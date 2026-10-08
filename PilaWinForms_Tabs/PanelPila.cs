using System;
using System.Drawing;
using System.Windows.Forms;
using PilaRepo.Shared;

namespace PilaWinForms_Tabs
{
    /// <summary>
    /// Panel reutilizable para una Pila&lt;T&gt; (estática, dinámica o nativa).
    /// Un botón independiente por operación; cada uno refresca la grilla y escribe en la
    /// etiqueta de estado la acción exacta realizada (o el error en rojo).
    /// </summary>
    internal class PanelPila<T> : TableLayoutPanel, IRefrescable
    {
        private Pila<T> _pila;
        private readonly string[] _columnas;
        private readonly Func<string[], T> _crearParaPush;
        private readonly Func<string[], T> _crearParaBuscar;
        private readonly Func<T, string[]> _aCeldas;
        private readonly TextBox[] _entradas;
        private readonly FlowLayoutPanel _encabezado;
        private readonly DataGridView _grid;
        private readonly Label _lblTop;
        private readonly Label _lblEstado;

        public PanelPila(Pila<T> pila, string[] columnas, string[] etiquetasEntrada, string[]? valoresIniciales,
                         Func<string[], T> crearParaPush, Func<string[], T> crearParaBuscar,
                         Func<T, string[]> aCeldas, string textoContains = "Contains")
        {
            _pila = pila;
            _columnas = columnas;
            _crearParaPush = crearParaPush;
            _crearParaBuscar = crearParaBuscar;
            _aCeldas = aCeldas;

            Dock = DockStyle.Fill;
            Padding = new Padding(8);
            ColumnCount = 1;
            RowCount = 6;
            ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            RowStyles.Add(new RowStyle(SizeType.AutoSize));          // 0 encabezado opcional
            RowStyles.Add(new RowStyle(SizeType.AutoSize));          // 1 entradas
            RowStyles.Add(new RowStyle(SizeType.AutoSize));          // 2 botones
            RowStyles.Add(new RowStyle(SizeType.AutoSize));          // 3 top = N
            RowStyles.Add(new RowStyle(SizeType.Percent, 100));      // 4 grilla
            RowStyles.Add(new RowStyle(SizeType.Absolute, 48));      // 5 estado

            _encabezado = UiHelper.NuevoFlow();
            _encabezado.Visible = false;

            var panelEntradas = UiHelper.NuevoFlow();
            _entradas = new TextBox[etiquetasEntrada.Length];
            for (int i = 0; i < etiquetasEntrada.Length; i++)
            {
                panelEntradas.Controls.Add(UiHelper.Etiqueta(etiquetasEntrada[i]));
                var tb = new TextBox { Width = 180, Margin = new Padding(3, 5, 3, 3) };
                if (valoresIniciales != null && i < valoresIniciales.Length) tb.Text = valoresIniciales[i];
                _entradas[i] = tb;
                panelEntradas.Controls.Add(tb);
            }

            var panelBotones = UiHelper.NuevoFlow();
            panelBotones.Controls.Add(UiHelper.Boton("Push", (s, e) => OnPush()));
            panelBotones.Controls.Add(UiHelper.Boton("Pop", (s, e) => OnPop()));
            panelBotones.Controls.Add(UiHelper.Boton("Peek", (s, e) => OnPeek()));
            panelBotones.Controls.Add(UiHelper.Boton("Count", (s, e) => OnCount()));
            panelBotones.Controls.Add(UiHelper.Boton("Clear", (s, e) => OnClear()));
            panelBotones.Controls.Add(UiHelper.Boton(textoContains, (s, e) => OnContains(), 140));

            _lblTop = new Label { AutoSize = true, Margin = new Padding(3, 8, 3, 4) };
            _lblTop.Font = new Font(_lblTop.Font, FontStyle.Bold);

            _grid = UiHelper.CrearGrid(columnas);

            _lblEstado = new Label { Dock = DockStyle.Fill, AutoSize = false };

            Controls.Add(_encabezado, 0, 0);
            Controls.Add(panelEntradas, 0, 1);
            Controls.Add(panelBotones, 0, 2);
            Controls.Add(_lblTop, 0, 3);
            Controls.Add(_grid, 0, 4);
            Controls.Add(_lblEstado, 0, 5);

            Refrescar();
        }

        /// <summary>Pila que muestra el panel (se puede reemplazar, p. ej. al reiniciar la estática).</summary>
        public Pila<T> PilaActual
        {
            get => _pila;
            set { _pila = value; Refrescar(); }
        }

        /// <summary>Agrega un control a la fila superior (usado por la pestaña estática).</summary>
        public void AgregarEncabezado(Control control)
        {
            _encabezado.Visible = true;
            _encabezado.Controls.Add(control);
        }

        public void Estado(string texto, bool error = false) => UiHelper.MostrarEstado(_lblEstado, texto, error);

        public void Refrescar()
        {
            UiHelper.LlenarGrid(_grid, _pila.ObtenerCopia(),
                                _pila.Modo == ModoPila.Estatica ? _pila.Capacidad : 0,
                                _aCeldas, _columnas.Length);

            string txt = $"top = {_pila.Top}   |   Count = {_pila.Count}";
            if (_pila.Modo == ModoPila.Estatica) txt += $"   |   tamaño = {_pila.Capacidad}";
            _lblTop.Text = txt;
        }

        // ------------------------------------------------------------------

        private string[] LeerCampos()
        {
            var r = new string[_entradas.Length];
            for (int i = 0; i < r.Length; i++) r[i] = _entradas[i].Text;
            return r;
        }

        private string Describir(T valor) => string.Join(" / ", _aCeldas(valor));

        private void Ejecutar(Func<string> accion)
        {
            try
            {
                Estado(accion());
            }
            catch (InvalidOperationException ex)      // Stack Overflow / Stack Underflow
            {
                Estado("Error: " + ex.Message, true);
            }
            catch (FormatException ex)                // entrada inválida
            {
                Estado("Entrada inválida: " + ex.Message, true);
            }
            Refrescar();
        }

        private void OnPush() => Ejecutar(() =>
        {
            T v = _crearParaPush(LeerCampos());
            _pila.Push(v);
            return $"Push({Describir(v)}) -> apilado en la cima. Count = {_pila.Count}";
        });

        private void OnPop() => Ejecutar(() =>
        {
            T v = _pila.Pop();
            return $"Pop() -> se retiró de la cima: {Describir(v)}. Count = {_pila.Count}";
        });

        private void OnPeek() => Ejecutar(() =>
        {
            T v = _pila.Peek();
            return $"Peek() -> la cima es: {Describir(v)} (no se retiró)";
        });

        private void OnCount() => Ejecutar(() => $"Count = {_pila.Count}");

        private void OnClear() => Ejecutar(() =>
        {
            _pila.Clear();
            return "Clear() -> pila vacía (Count = 0)";
        });

        private void OnContains() => Ejecutar(() =>
        {
            T buscado = _crearParaBuscar(LeerCampos());
            bool r = _pila.Contains(buscado);
            return $"Contains({Describir(buscado)}) -> {(r ? "TRUE: está en la pila" : "FALSE: no está en la pila")}";
        });
    }
}
