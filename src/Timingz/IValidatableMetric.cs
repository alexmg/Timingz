namespace Timingz;

internal interface IValidatableMetric
{
    bool Validate(out string message);
}