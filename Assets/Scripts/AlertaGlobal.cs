using System;
using UnityEngine;

public static class AlertaGlobal
{
    public static Action<Vector3> OnAlertaGlobal;

    public static void Activar(Vector3 puntoDeteccion)
    {
        OnAlertaGlobal?.Invoke(puntoDeteccion);
    }
}