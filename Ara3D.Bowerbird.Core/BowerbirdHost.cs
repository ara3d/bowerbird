using System;
using Ara3D.Bowerbird.Interfaces;

namespace Ara3D.Bowerbird.Core
{
    public class BowerbirdHost : IBowerbirdHost
    {
        public Action<IBowerbirdCommand> Action;

        public BowerbirdHost(Action<IBowerbirdCommand> action)
            => Action = action;

        public void ExecuteCommand(IBowerbirdCommand cmd)
            => Action.Invoke(cmd);

        public static IBowerbirdHost Create(Action<IBowerbirdCommand> action)
            => new BowerbirdHost(action);

        public static IBowerbirdHost CreateDefault()
            => new BowerbirdHost(cmd => cmd.Execute(null));
    }
}