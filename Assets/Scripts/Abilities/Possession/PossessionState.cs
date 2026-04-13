namespace Possession
{
    public enum PossessionState
    {
        Free,       // El jugador se mueve con normalidad
        Scanning,   // La habilidad está activa, objetos resaltados
        Possessing  // El jugador controla un objeto
    }
}