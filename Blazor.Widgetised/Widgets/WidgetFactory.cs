﻿using System;
using System.Collections.Generic;
using Blazor.Widgetised.Logging;
using Blazor.Widgetised.Mediators;
using Blazor.Widgetised.Messaging;
using Blazor.Widgetised.Presenters;
using Microsoft.Extensions.DependencyInjection;

namespace Blazor.Widgetised
{
    public class WidgetFactory : IWidgetFactory
    {
        private readonly IServiceProvider provider;
        private readonly IWidgetStateStore stateStore;
        private readonly IDictionary<string, WidgetVariant> map;

        public WidgetFactory(IWidgetStateStore stateStore, IServiceProvider provider)
        {
            this.stateStore = stateStore;
            this.provider = provider;

            map = new Dictionary<string, WidgetVariant>();
        }

        public void Register(string variantKey, WidgetVariant variant)
        {
            map[variantKey] = variant;
        }

        public object Build(string variantKey)
        {
            return Build(new WidgetDescription { VariantName = variantKey });
        }

        public object Build(WidgetDescription description)
        {
            if (string.IsNullOrEmpty(description.VariantName))
            {
                if (!map.TryGetValue(description.VariantName, out WidgetVariant variant))
                {
                    ConsoleLogger.Debug($"WARNING: No widget variant for the key '{description.VariantName}'.");
                }

                description.Variant = variant;
            }

            return BuildMediator(description);
        }

        private object BuildMediator(WidgetDescription description)
        {
            if (description.Variant == null)
            {
                return Guid.Empty;
            }

            Type mediatorType = description.Variant.MediatorType;
            object mediator = provider.GetService(mediatorType);

            TryFillMediatorContract(mediator, description);
            TryInitialise(mediator);

            if (mediator == null)
            {
                ConsoleLogger.Debug($"ERROR: No mediator for a widget of type '{description.Variant.MediatorType.Name}'.");
            }

            return mediator;
        }

        private void TryFillMediatorContract(object mediator, WidgetDescription description)
        {
            if (!(mediator is IWidgetMediatorBuildContract contract))
            {
                return;
            }

            contract.SetMessageBus(provider.GetService<IMessageBus>());
            contract.SetCustomisation(description.Variant.Customisation);

            if (TryGetState(description, out object state))
            {
                contract.SetState(state);
            }

            if (!TryGetPresenter(description.Variant.PresenterType, out IWidgetPresenter presenter)
                && mediator is IWidgetPresenterProvider presenterProvider)
            {
                // TODO: solve this in more elegant way
                presenter = presenterProvider.Presenter;
            }

            TryFillPresenterContract(presenter);
            TryInitialise(presenter);

            if (presenter == null)
            {
                ConsoleLogger.Debug($"WARNING: No presenter for a widget of type '{description.Variant.MediatorType.Name}'.");
            }

            contract.SetPresenter(presenter);
        }

        private void TryFillPresenterContract(IWidgetPresenter presenter)
        {
            if (!(presenter is IWidgetPresenterBuildContract contract))
            {
                return;
            }

            contract.SetWidgetContainerManagement(provider.GetService<IWidgetContainerManagement>());
        }

        private bool TryGetState(WidgetDescription description, out object state)
        {
            if (description.Variant.StateType == null)
            {
                state = null;
                return false;
            }

            bool stateCanBeStored = !string.IsNullOrEmpty(description.Position);
            string stateKey = BuildWidgetKey(description); // TODO: optimise this

            if (stateCanBeStored
                && (state = stateStore.Get(stateKey)) != null)
            {
                return true;
            }

            state = provider.GetService(description.Variant.StateType);

            if (state != null && stateCanBeStored)
            {
                stateStore.Add(stateKey, state);
            }

            return true;
        }

        private bool TryGetPresenter(Type presenterType, out IWidgetPresenter presenter)
        {
            if (presenterType == null)
            {
                presenter = null;
                return false;
            }

            presenter = (IWidgetPresenter)provider.GetService(presenterType);
            return true;
        }

        private static void TryInitialise(object subject)
        {
            if (!(subject is IInitialisable initialisable))
            {
                return;
            }

            initialisable.Initialise();
        }

        private static string BuildWidgetKey(IWidgetIdentifier description)
        {
            return description.GetKey();
        }
    }
}