using UnityEngine;

/// <summary>
/// Interface simples para receber dano sem precisar da direção do impacto.
/// Ideal para armas corpo a corpo, magias em área ou bosses.
/// </summary>
public interface IDamageableSimple
{
    void TakeDamage(float amount);
}
