
using System;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace Ro.Data.SqlClient
{
    public class AopAttribute : ProxyAttribute
    {
        public override MarshalByRefObject CreateInstance(Type serverType)
        {
            AOPProxy realProxy = new AOPProxy(serverType, base.CreateInstance(serverType));
            return realProxy.GetTransparentProxy() as MarshalByRefObject;
        }
    }

    public class AOPProxy : RealProxy
    {
        public MethodInfo method;
        MarshalByRefObject _target = null;

        public AOPProxy(Type serverType, MarshalByRefObject target)
            : base(serverType)
        {
            _target = target;
            method = serverType.GetMethod("SetChange");
        }
        public override IMessage Invoke(IMessage msg)
        {
            if (msg is IConstructionCallMessage)
            {
                IConstructionCallMessage constructCallMsg = msg as IConstructionCallMessage;

                RealProxy defaultProxy = RemotingServices.GetRealProxy(_target);
                defaultProxy.InitializeServerObject(constructCallMsg);
                return System.Runtime.Remoting.Services.EnterpriseServicesHelper.CreateConstructionReturnMessage(constructCallMsg, (MarshalByRefObject)GetTransparentProxy());
            }
            else if (msg is IMethodCallMessage)
            {
                IMethodCallMessage callMsg = msg as IMethodCallMessage;
                object[] args = callMsg.Args;

                if (callMsg.MethodName.StartsWith("set_") && args.Length == 1)
                {
                    if (callMsg.MethodName.StartsWith("set_") && args.Length == 1)
                    {
                        method.Invoke(_target, new object[] { callMsg.MethodName.Substring(4), args[0] });
                    }
                }
                return RemotingServices.ExecuteMessage(_target, callMsg);
            }
            return msg;
        }
    }
}
