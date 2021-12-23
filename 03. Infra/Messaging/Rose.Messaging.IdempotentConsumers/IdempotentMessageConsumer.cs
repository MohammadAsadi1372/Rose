﻿using Rose.Core.ApplicationServices.Events;
using Rose.Utilities.Configurations;
using Rose.Utilities.Services.MessageBus;
using Rose.Utilities.Services.Serializers;
using Rose.Core.Contracts.ApplicationServices.Commands;

namespace Rose.Messaging.IdempotentConsumers;
public class IdempotentMessageConsumer : IMessageConsumer
{
    private readonly RoseConfigurationOptions _RoseConfigurations;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IMessageInboxItemRepository _messageInboxItemRepository;
    private readonly Dictionary<string, string> _messageTypeMap = new Dictionary<string, string>();
    public IdempotentMessageConsumer(RoseConfigurationOptions RoseConfigurations, IEventDispatcher eventDispatcher, IJsonSerializer jsonSerializer, ICommandDispatcher commandDispatcher, IMessageInboxItemRepository messageInboxItemRepository)
    {
        _RoseConfigurations = RoseConfigurations;
        _eventDispatcher = eventDispatcher;
        _jsonSerializer = jsonSerializer;
        _commandDispatcher = commandDispatcher;
        _messageInboxItemRepository = messageInboxItemRepository;
        LoadMessageMap();
    }

    private void LoadMessageMap()
    {

        if (_RoseConfigurations?.Messageconsumer?.Events?.Any() == true)
        {
            foreach (var eventPublisher in _RoseConfigurations?.Messageconsumer?.Events)
            {
                foreach (var @event in eventPublisher?.EventData)
                {
                    _messageTypeMap.Add($"{eventPublisher.FromServiceId}.{@event.EventName}", @event.MapToClass);

                }
            }
        }
        if (_RoseConfigurations?.Messageconsumer?.Commands?.Any() == true)
        {
            foreach (var item in _RoseConfigurations?.Messageconsumer?.Commands)
            {
                _messageTypeMap.Add($"{_RoseConfigurations.ServiceId}.{item.CommandName}", item.MapToClass);
            }
        }
    }

    public void ConsumeCommand(string sender, Parcel parcel)
    {
        if (_messageInboxItemRepository.AllowReceive(parcel.MessageId, sender))
        {
            var mapToClass = _messageTypeMap[parcel.Route];
            var commandType = Type.GetType(mapToClass);
            dynamic command = _jsonSerializer.Deserialize(parcel.MessageBody, commandType);
            _commandDispatcher.Send(command);
            _messageInboxItemRepository.Receive(parcel.MessageId, sender);
        }
    }

    public void ConsumeEvent(string sender, Parcel parcel)
    {
        if (_messageInboxItemRepository.AllowReceive(parcel.MessageId, sender))
        {
            var mapToClass = _messageTypeMap[parcel.Route];
            var eventType = Type.GetType(mapToClass);
            dynamic @event = _jsonSerializer.Deserialize(parcel.MessageBody, eventType);
            _eventDispatcher.PublishDomainEventAsync(@event);
            _messageInboxItemRepository.Receive(parcel.MessageId, sender);
        }
    }
}
