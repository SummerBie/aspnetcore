// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace Microsoft.AspNetCore.Components.Web.Extensions
{
    public class Drop<TItem> : ComponentBase, IAsyncDisposable
    {
        private long _id;

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        [Parameter]
        public Action<TItem, DragEventArgs>? OnDrop { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var interopRelayReference = DotNetObjectReference.Create(new DropInteropRelay<TItem>(this));
            _id = await JSRuntime.InvokeAsync<long>("_blazorDragAndDrop.registerDrop", interopRelayReference);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "ondrop", $"window._blazorDragAndDrop.onDrop(event, {_id})");
            builder.AddAttribute(2, "ondragover", $"window._blazorDragAndDrop.onDragOver(event, {_id})");
            builder.AddContent(3, ChildContent);
            builder.CloseElement();
        }

        internal void OnDropCore()
        {
            // TODO: Some manual state management might be required to track which object(s) are being dragged (target and currentTarget are not relevant)
            //OnDrop?.Invoke(default, null);
        }

        public ValueTask DisposeAsync()
            => JSRuntime.InvokeVoidAsync("_blazorDragAndDrop.unregisterDrop", _id);
    }
}
