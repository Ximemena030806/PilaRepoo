using System;
using System.Drawing;
using System.Windows.Forms;
using PilaRepo.Shared;

namespace PilaWinForms_Tabs
{
    /// <summary>Panel de la pila de contactos (List&lt;Contacto&gt; usada como pila), precargada con 3 contactos.</summary>
    internal class PanelContactos : TableLayoutPanel, IRefrescable
    {
        private readonly PilaContactos _pila = new PilaContactos();
        private readonly TextBox _txtNombre = new TextBox { Width = 140 };
        private readonly TextBox _txtTelefono = new TextBox { Width = 110 };
        private readonly TextBox _txtDireccion = new TextBox { Width = 170 };
        private readonly TextBox _txtCorreo = new TextBox { Width = 160 };
        private readonly TextBox _txtBuscar = new TextBox { Width = 160 };
        private readonly DataGridView _grid;
        private readonly Label _lblTop;
        private readonly Label _lblEstado;

        public PanelContactos()
        {
            Dock = DockStyle.Fill;
            Padding = new Padding(8);
            ColumnCount = 1;
            RowCount = 6;
            ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            RowStyles.Add(new RowStyle(SizeType.AutoSize));          // entradas
            RowStyles.Add(new RowStyle(SizeType.AutoSize));          // botones
            RowStyles.Add(new RowStyle(SizeType.AutoSize));          // búsqueda
            RowStyles.Add(new RowStyle(SizeType.AutoSize));          // top = N
            RowStyles.Add(new RowStyle(SizeType.Percent, 100));      // grilla
            RowStyles.Add(new RowStyle(SizeType.Absolute, 48));      // estado

            var entradas = UiHelper.NuevoFlow();
            AgregarCampo(entradas, "Nombre:", _txtNombre);
            AgregarCampo(entradas, "Teléfono:", _txtTelefono);
            AgregarCampo(entradas, "Dirección:", _txtDireccion);
            AgregarCampo(entradas, "Correo:", _txtCorreo);

            var botones = UiHelper.NuevoFlow();
            botones.Controls.Add(UiHelper.Boton("Push", (s, e) => OnPush()));
            botones.Controls.Add(UiHelper.Boton("Pop", (s, e) => OnPop()));
            botones.Controls.Add(UiHelper.Boton("Peek", (s, e) => OnPeek()));
            botones.Controls.Add(UiHelper.Boton("Count", (s, e) => OnCount()));
            botones.Controls.Add(UiHelper.Boton("Clear", (s, e) => OnClear()));

            var busqueda = UiHelper.NuevoFlow();
            AgregarCampo(busqueda, "Buscar nombre:", _txtBuscar);
            busqueda.Controls.Add(UiHelper.Boton("Contains (por nombre)", (s, e) => OnContains(), 160));

            _lblTop = new Label { AutoSize = true, Margin = new Padding(3, 8, 3, 4) };
            _lblTop.Font = new Font(_lblTop.Font, FontStyle.Bold);

            _grid = UiHelper.CrearGrid("Nombre", "Teléfono", "Dirección", "Correo");
            _lblEstado = new Label { Dock = DockStyle.Fill, AutoSize = false };

            Controls.Add(entradas, 0, 0);
            Controls.Add(botones, 0, 1);
            Controls.Add(busqueda, 0, 2);
            Controls.Add(_lblTop, 0, 3);
            Controls.Add(_grid, 0, 4);
            Controls.Add(_lblEstado, 0, 5);

            Refrescar();
        }

        private static void AgregarCampo(FlowLayoutPanel panel, string etiqueta, TextBox tb)
        {
            panel.Controls.Add(UiHelper.Etiqueta(etiqueta));
            tb.Margin = new Padding(3, 5, 3, 3);
            panel.Controls.Add(tb);
        }

        private static string[] Celdas(Contacto c) => new[] { c.Nombre, c.Telefono, c.Direccion, c.Correo };

        public void Refrescar()
        {
            UiHelper.LlenarGrid(_grid, _pila.ObtenerCopia(), 0, Celdas, 4);
            _lblTop.Text = $"top = {_pila.Count - 1}   |   Count = {_pila.Count}";
        }

        private void Estado(string texto, bool error = false) => UiHelper.MostrarEstado(_lblEstado, texto, error);

        private void Ejecutar(Func<string> accion)
        {
            try { Estado(accion()); }
            catch (InvalidOperationException ex) { Estado("Error: " + ex.Message, true); }
            catch (FormatException ex) { Estado("Entrada inválida: " + ex.Message, true); }
            Refrescar();
        }

        private void OnPush() => Ejecutar(() =>
        {
            if (string.IsNullOrWhiteSpace(_txtNombre.Text))
                throw new FormatException("El nombre es obligatorio.");
            var c = new Contacto(_txtNombre.Text, _txtTelefono.Text, _txtDireccion.Text, _txtCorreo.Text);
            _pila.Push(c);
            return $"Push({c.Nombre}) -> apilado en la cima. Count = {_pila.Count}";
        });

        private void OnPop() => Ejecutar(() =>
        {
            Contacto c = _pila.Pop();
            return $"Pop() -> se retiró de la cima: {c.Nombre}. Count = {_pila.Count}";
        });

        private void OnPeek() => Ejecutar(() =>
        {
            Contacto c = _pila.Peek();
            return $"Peek() -> la cima es: {c.Nombre} (no se retiró)";
        });

        private void OnCount() => Ejecutar(() => $"Count = {_pila.Count}");

        private void OnClear() => Ejecutar(() =>
        {
            _pila.Clear();
            return "Clear() -> pila vacía (Count = 0)";
        });

        private void OnContains() => Ejecutar(() =>
        {
            bool r = _pila.Contains(_txtBuscar.Text);
            return $"Contains(\"{_txtBuscar.Text.Trim()}\") -> {(r ? "TRUE: está en la pila" : "FALSE: no está en la pila")}";
        });
    }
}
