using System.Diagnostics.CodeAnalysis;

namespace Timingz;

internal interface IValidatableMetric
{
    bool Validate([NotNullWhen(false)] out string? message);
}