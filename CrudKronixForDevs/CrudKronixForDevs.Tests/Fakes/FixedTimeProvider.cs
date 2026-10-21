using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Tests.Fakes
{
    /// <summary>
    /// Relógio "parado" numa data fixa.
    ///
    /// Por que existe: as regras de validade dependem da data de hoje. Se o teste usasse o
    /// relógio real, ele poderia passar hoje e falhar daqui a um ano. Com o relógio fixo,
    /// o resultado é sempre o mesmo, em qualquer dia e em qualquer máquina.
    /// </summary>
    public class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        /// <summary>Data padrão usada nos testes: 15/01/2026 às 10h (UTC).</summary>
        public static readonly DateTimeOffset DefaultNow = new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

        public FixedTimeProvider() : this(DefaultNow) { }

        public override DateTimeOffset GetUtcNow() => now;

        // Fuso UTC para que GetLocalNow() devolva exatamente a mesma data em qualquer máquina.
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

        /// <summary>A "data de hoje" deste relógio.</summary>
        public DateOnly Today => DateOnly.FromDateTime(now.UtcDateTime);
    }
}
