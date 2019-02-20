﻿using Blazor.Widgetised.Messaging;
using Blazor.Widgetised.Presenters;

namespace Blazor.Widgetised.Mediators
{
    public interface IWidgetMediatorBuildContract
    {
        void SetPresenter(IWidgetPresenter presenter);

        void SetCustomisation(object customisation);

        void SetState(object state);

        void SetMessageBus(IMessageBus bus);
    }
}