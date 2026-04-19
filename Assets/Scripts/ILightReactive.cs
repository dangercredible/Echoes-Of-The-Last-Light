/// <summary>
/// Contract for objects that can be illuminated by the lantern system.
/// </summary>
public interface ILightReactive
{
    bool IsIlluminated { get; }
    void SetIlluminated(bool illuminated);
}
