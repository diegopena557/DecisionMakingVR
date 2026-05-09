public enum EncuestaVASModo
{
    Ninguno = 0,
    AntesExperiencia = 1,
    DespuesExperiencia = 2
}

public static class EncuestaVASFlow
{
    public static EncuestaVASModo Modo { get; private set; } = EncuestaVASModo.Ninguno;
    public static string SiguienteEscena { get; private set; } = "";

    public static void PrepararAntesExperiencia(string siguienteEscena)
    {
        Modo = EncuestaVASModo.AntesExperiencia;
        SiguienteEscena = siguienteEscena;
    }

    public static void PrepararDespuesExperiencia(string siguienteEscena)
    {
        Modo = EncuestaVASModo.DespuesExperiencia;
        SiguienteEscena = siguienteEscena;
    }

    public static void Limpiar()
    {
        Modo = EncuestaVASModo.Ninguno;
        SiguienteEscena = "";
    }
}