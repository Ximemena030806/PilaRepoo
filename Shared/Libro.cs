using System;

namespace PilaRepo.Shared
{
    /// <summary>Libro de la pila dinámica. Igualdad definida por Titulo.</summary>
    internal class Libro : IEquatable<Libro>
    {
        public string Titulo { get; }
        public string Autor { get; }

        public Libro(string titulo, string autor)
        {
            Titulo = (titulo ?? string.Empty).Trim();
            Autor = (autor ?? string.Empty).Trim();
        }

        // Equals / GetHashCode SOLO por Titulo (sin distinguir mayúsculas).
        public bool Equals(Libro? other) =>
            other is not null && string.Equals(Titulo, other.Titulo, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object? obj) => Equals(obj as Libro);

        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Titulo);

        public override string ToString() => $"{Titulo} / {Autor}";

        /// <summary>
        /// Pila dinámica PRECARGADA de las diapositivas: El Principito, 1984 y
        /// Cien Años de Soledad (este último queda como tope).
        /// Está aquí para tener UNA sola copia compartida por WinForms y consola.
        /// </summary>
        public static Pila<Libro> CrearPilaPrecargada()
        {
            var pila = new Pila<Libro>();
            pila.Push(new Libro("El Principito", "A. de Saint-Exupéry"));
            pila.Push(new Libro("1984", "George Orwell"));
            pila.Push(new Libro("Cien Años de Soledad", "G. García Márquez"));
            return pila;
        }
    }
}
