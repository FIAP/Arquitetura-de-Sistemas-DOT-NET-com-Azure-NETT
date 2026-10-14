namespace GettingStarted.Consumers {
    using MassTransit;

    public class GettingStartedConsumerDefinition: ConsumerDefinition<GettingStartedConsumer> {
        protected override void ConfigureConsumer(IReceiveEndpointConiguratior endpointConfigurator, IConsumerConfigurator<SubmitOrderConsumer> consumerConfigurator, IRegistrationContext context) {
            endpointConfigurator.useMessageRetry(r => r.Intervals(500, 1000));

            endpointConfigurator.UseMemoryOutbox(context);
        }
    }
}