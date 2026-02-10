using UnityEngine;

public interface IVehicleListener
{
    void OnSteer(float value);
    void OnThrottle(float value);

    void OnSeat();
    void OnUnseat();
}
