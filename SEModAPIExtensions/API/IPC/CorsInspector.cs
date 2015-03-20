using System;
using System.Collections.Generic;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace SEModAPIExtensions.API.IPC
{
	using System.Collections.ObjectModel;

	public class CorsInspector : IDispatchMessageInspector
	{
		Dictionary<string, string> requiredHeaders;

		public CorsInspector( Dictionary<string, string> headers )
		{
			requiredHeaders = headers ?? new Dictionary<string, string>( );
		}

		public object AfterReceiveRequest( ref Message request, IClientChannel channel, InstanceContext instanceContext )
		{
			return null;
		}

		public void BeforeSendReply( ref Message reply, object correlationState )
		{
			var httpHeader = reply.Properties[ "httpResponse" ] as HttpResponseMessageProperty;
			foreach ( var item in requiredHeaders )
			{
				httpHeader.Headers.Add( item.Key, item.Value );
			}
		}
	}

	[AttributeUsage( AttributeTargets.Class )]
	public class EnableCorsBehavior : Attribute, IServiceBehavior
	{
		public void AddBindingParameters( ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters )
		{

		}

		public void ApplyDispatchBehavior( ServiceDescription serviceDescription, ServiceHostBase serviceHostBase )
		{
			var requiredHeaders = new Dictionary<string, string>( );

			requiredHeaders.Add( "Access-Control-Allow-Origin", "*" );
			requiredHeaders.Add( "Access-Control-Request-Method", "POST,GET,PUT,DELETE,OPTIONS" );
			requiredHeaders.Add( "Access-Control-Allow-Headers", "X-Requested-With,Content-Type" );

			foreach ( ChannelDispatcher cd in serviceHostBase.ChannelDispatchers )
			{
				foreach ( EndpointDispatcher ed in cd.Endpoints )
				{
					ed.DispatchRuntime.MessageInspectors.Add( new CorsInspector( requiredHeaders ) );
				}
			}
		}

		public void Validate( ServiceDescription serviceDescription, ServiceHostBase serviceHostBase )
		{

		}
	}
}
