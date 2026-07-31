using System;

namespace Homework.Routing
{
    public static class SimpleMessages
    {
        public class ExchangeAdded : IMessage
        {
            public int ExchangeId { get; set; }
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
        }

        public class ExchangeTradingPhaseChanged : IMessage
        {
            public int ExchangeId { get; set; }
            public int TradingPhaseId { get; set; }
            public DateTime TimestampUtc { get; set; }
        }
    }
}
