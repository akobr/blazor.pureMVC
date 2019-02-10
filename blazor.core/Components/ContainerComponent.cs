﻿using Blazor.Core.Widgets;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using System;

namespace Blazor.Core.Components
{
    public class ContainerComponent : ComponentBase, IRenderingContainer, IDisposable
    {
        private RenderFragment content;
        private string registeredKey;

        [Inject]
        protected IWidgetContainerManagement Management { get; set; }

        [Parameter]
        private string Key { get; set; }

        public void SetKey(string newKey)
        {
            if (string.Equals(Key, newKey, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            Key = newKey;
            StateHasChanged();
        }

        public void SetContent(RenderFragment content)
        {
            this.content = content;
            StateHasChanged();
        }

        public void Dispose()
        {
            UnregisterKey();
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);

            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "container");
            
            if (!string.IsNullOrEmpty(Key))
            {
                builder.AddAttribute(2, "data-key", Key);
                RegisterKey(Key);
            }
            
            if (content != null)
            {
                builder.AddContent(3, content);
            }

            builder.CloseElement();
        }

        private void RegisterKey(string key)
        {
            if (Management == null)
            {
                return;
            }

            UnregisterKey();

            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            Management.Register(key, this);
            registeredKey = key;
        }

        private void UnregisterKey()
        {
            if (Management == null
                || string.IsNullOrEmpty(registeredKey))
            {
                return;
            }

            Management.Unregister(registeredKey);
            registeredKey = null;
        }
    }
}
