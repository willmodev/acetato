namespace Acetato.Domain;

/// <summary>
/// Mapea la edad de un punto de la estela del láser a su opacidad. Recién creado
/// (edad 0) es totalmente opaco (1.0); al alcanzar su vida se vuelve transparente
/// (0.0), con desvanecido lineal entre ambos extremos. Tipo puro (sin WPF), testeable.
/// </summary>
public static class LaserFade
{
    /// <summary>
    /// Opacidad [0..1] de un punto de la estela según su edad y su vida, ambas en ms.
    /// Edad 0 → 1.0; edad ≥ vida → 0.0; clamp fuera de rango; monotonía decreciente.
    /// Una vida no positiva colapsa el punto a 0.0 (ya expirado).
    /// </summary>
    public static double OpacityForAge(double ageMs, double lifetimeMs)
    {
        if (lifetimeMs <= 0d)
        {
            return 0d;
        }

        return Math.Clamp(1d - ageMs / lifetimeMs, 0d, 1d);
    }
}
