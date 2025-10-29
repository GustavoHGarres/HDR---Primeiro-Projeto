using UnityEngine;

public interface IDamageable
{
    /// <param name="amount">quanto de dano</param>
    /// <param name="hitPoint">onde acertou (mundo)</param>
    /// <param name="hitDir">direção do impacto (normalizada)</param>
    void Damage(float amount, Vector3 hitPoint, Vector3 hitDir);
}
