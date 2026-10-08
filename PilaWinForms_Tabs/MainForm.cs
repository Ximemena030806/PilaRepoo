using System;
using System.Drawing;
using System.Windows.Forms;
using PilaRepo.Shared;

namespace PilaWinForms_Tabs
{
    /// <summary>Ventana principal: TabControl con 5 pestañas.</summary>
    internal class MainForm : Form
    {
        private readonly TabControl _tabs = new TabControl { Dock = DockStyle.Fill };

        public MainForm()
        {
            Text = "Pila: estática, dinámica, nativa, List<T> y comparación";
            ClientSize = new Size(1000, 660);
            MinimumSize = new Size(820, 560);
            StartPosition = FormStartPosition.CenterScreen;

            _tabs.TabPages.Add(CrearTabEstatica());
            _tabs.TabPages.Add(CrearTabDinamica());
            _tabs.TabPages.Add(CrearTabNativa());
            _tabs.TabPages.Add(CrearTabContactos());
            _tabs.TabPages.Add(CrearTabComparacion());

            // Al cambiar de pestaña se refresca la grilla visible.
            _tabs.SelectedIndexChanged += (s, e) =>
            {
                if (_tabs.SelectedTab != null)
                    foreach (Control c in _tabs.SelectedTab.Controls)
                        if (c is IRefrescable r) r.Refrescar();
            };

            Controls.Add(_tabs);
        }

        private static TabPage NuevaPagina(string titulo, Control contenido)
        {
            var tp = new TabPage(titulo);
            tp.Controls.Add(contenido);
            return tp;
        }

        private static int LeerEntero(string[] campos)
        {
            if (!int.TryParse(campos[0].Trim(), out int v))
                throw new FormatException("escribe un número entero válido.");
            return v;
        }

        // 1) Pila Estática: Pila<int> de tamaño fijo (por defecto 4, índices 0-3), vacía con top = -1.
        private TabPage CrearTabEstatica()
        {
            var panel = new PanelPila<int>(new Pila<int>(4), new[] { "Valor" }, new[] { "Valor (int):" },
                                           new[] { "10" }, LeerEntero, LeerEntero, v => new[] { v.ToString() });

            var num = new NumericUpDown { Minimum = 1, Maximum = 50, Value = 4, Width = 60, Margin = new Padding(3, 5, 3, 3) };
            panel.AgregarEncabezado(UiHelper.Etiqueta("Tamaño de la pila:"));
            panel.AgregarEncabezado(num);
            panel.AgregarEncabezado(UiHelper.Boton("Crear / Reiniciar Pila", (s, e) =>
            {
                panel.PilaActual = new Pila<int>((int)num.Value);
                panel.Estado($"Pila estática creada: tamaño {(int)num.Value} (índices 0-{(int)num.Value - 1}), top = -1");
            }, 160));

            return NuevaPagina("Pila Estática", panel);
        }

        // 2) Pila Dinámica: Pila<Libro> PRECARGADA con 3 libros (el último es el tope).
        private TabPage CrearTabDinamica()
        {
            var panel = new PanelPila<Libro>(Libro.CrearPilaPrecargada(),
                new[] { "Título", "Autor" }, new[] { "Título:", "Autor:" },
                new[] { "Don Quijote de la Mancha", "Miguel de Cervantes" },
                c => new Libro(c[0], c[1]),
                c => new Libro(c[0], ""),                  // Contains busca solo por título
                l => new[] { l.Titulo, l.Autor },
                "Contains (por título)");
            return NuevaPagina("Pila Dinámica (libros)", panel);
        }

        // 3) Pila Nativa: Pila<int> respaldada por Stack<T>, vacía.
        private TabPage CrearTabNativa()
        {
            var panel = new PanelPila<int>(Pila<int>.CrearNativa(), new[] { "Valor" }, new[] { "Valor (int):" },
                                           new[] { "10" }, LeerEntero, LeerEntero, v => new[] { v.ToString() });
            return NuevaPagina("Pila Nativa (Stack<T>)", panel);
        }

        // 4) Pila de Contactos (List<T>), precargada con 3 contactos.
        private TabPage CrearTabContactos() => NuevaPagina("Pila de Contactos (List<T>)", new PanelContactos());

        // 5) Comparación de las tres pilas propias de la pestaña.
        private TabPage CrearTabComparacion() => NuevaPagina("Comparación", new PanelComparacion());
    }
}
