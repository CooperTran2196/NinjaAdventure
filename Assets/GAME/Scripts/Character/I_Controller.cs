using UnityEngine;

public interface I_Controller
{
    void SetDesiredVelocity(Vector2 velocity);
    void ReceiveKnockback(Vector2 impulse);
}
