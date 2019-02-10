﻿using Blazor.Client.Widgets.Counter;
using Blazor.PureMvc.Widgets;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Blazor.Client
{
    public static class WidgetRegister
    {
        public static void RegisterWidgets(this IServiceCollection services)
        {
            services.AddTransient<CounterWidgetMediator>();
            services.AddTransient<CounterWidgetPresenter>();
            services.AddTransient<CounterWidgetState>();
        }

        public static void RegisterWidgetVariants(this IComponentsApplicationBuilder appBuilder)
        {
            IWidgetFactory widgetFactory = appBuilder.Services.GetService<IWidgetFactory>();

            widgetFactory.Register("Counter", new WidgetVariant
            {
                MediatorType = typeof(CounterWidgetMediator),
                PresenterType = typeof(CounterWidgetPresenter),
                StateType = typeof(CounterWidgetState)
            });
        }
    }
}