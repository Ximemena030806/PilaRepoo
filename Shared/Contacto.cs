using System;

namespace PilaRepo.Shared
{
    /// <summary>Contacto de la pila basada en List&lt;T&gt;. Igualdad definida por Nombre.</summary>
    internal class Contacto : IEquatable<Contacto>
    {
        public string Nombre { get; }
        public string Telefono { get; }
        public string Direccion { get; }
        public string Correo { get; }

        public Contacto(string nombre, string telefono, string direccion, string correo)
        {
            Nombre = (nombre ?? string.Empty).Trim();
            Telefono = (telefono ?? string.Empty).Trim();
            Direccion = (direccion ?? string.Empty).Trim();
            Correo = (correo ?? string.Empty).Trim();
        }

        // Equals / GetHashCode SOLO por Nombre (sin distinguir mayúsculas).
        public bool Equals(Contacto? other) =>
            other is not null && string.Equals(Nombre, other.Nombre, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object? obj) => Equals(obj as Contacto);

        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Nombre);

        public override string ToString() => $"{Nombre} / {Telefono} / {Direccion} / {Correo}";
    }
}
