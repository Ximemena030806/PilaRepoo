using System;
using System.Drawing;
using System.Windows.Forms;
using PilaRepo.Shared;

namespace PilaWinForms_Tabs
{
    /// <summary>
    /// Pestaña de comparación: TRES Pila&lt;int&gt; PROPIAS de esta pestaña (Estática, Dinámica y
    /// Nativa), lado a lado, cada una con mini-grilla y Count. Los botones aplican la misma
    /// operación a las tres y se muestra cuál falló y por qué (Overflow / Underflow).
    /// </summary>
    internal class PanelComparacion : TableLayoutPanel, IRefrescable
    {
        private Pila<int> _estatica;
        private Pila<int> _dinamica;
        private Pila<int> _nativa;

        private readonly NumericUpDown _numValor = new NumericUpDown { Minimum = -100000, Maximum = 100000, Value = 10, Width = 80 };
        private readonly NumericUpDown _numTam = new NumericUpDown { Minimum = 1, Maximum = 50, Value = 4, Width = 60 };
        private readonly Label _lblEstado;

        private DataGridView _gridEst = null!, _gridDin = null!, _gridNat = null!;
        private Label _countEst = null!, _countDin = null!, _countNat = null!;
        private Label _errEst = null!, _errDin = null!, _errNat = null!;

        public PanelComparacion()
        {
            _estatica = new Pila<int>(4);
            _dinamica = new Pila<int>();
            _nativa = Pila<int>.CrearNativa();

            Dock = DockStyle.Fill;
            Padding = new Padding(8);
            ColumnCount = 1;
            RowCount = 3;
            ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            RowStyles.Add(new RowStyle(SizeType.AutoSize));          // controles
            RowStyles.Add(new RowStyle(SizeType.Percent, 100));      // las tres pilas
            RowStyles.Add(new RowStyle(SizeType.Absolute, 70));      // estado general

            var controles = UiHelper.NuevoFlow();
            controles.Controls.Add(UiHelper.Etiqueta("Valor:"));
            _numValor.Margin = new Padding(3, 5, 3, 3);
            controles.Controls.Add(_numValor);
            controles.Controls.Add(UiHelper.Boton("Push en las 3", (s, e) => OnPush(), 120));
            controles.Controls.Add(UiHelper.Boton("Pop en las 3", (s, e) => OnPop(), 120));
            controles.Controls.Add(UiHelper.Boton("Clear en las 3", (s, e) => OnClear(), 120));
            controles.Controls.Add(UiHelper.Etiqueta("Tamaño de la estática:"));
            _numTam.Margin = new Padding(3, 5, 3, 3);
            controles.Controls.Add(_numTam);
            controles.Controls.Add(UiHelper.Boton("Reiniciar las 3", (s, e) => OnReiniciar(), 120));

            var tres = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
            for (int i = 0; i < 3; i++) tres.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            tres.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tres.Controls.Add(CrearGrupo("Estática (arreglo + top)", out _gridEst, out _countEst, out _errEst), 0, 0);
            tres.Controls.Add(CrearGrupo("Dinámica (nodos + Prev)", out _gridDin, out _countDin, out _errDin), 1, 0);
            tres.Controls.Add(CrearGrupo("Nativa (Stack<T>)", out _gridNat, out _countNat, out _errNat), 2, 0);

            _lblEstado = new Label { Dock = DockStyle.Fill, AutoSize = false };

            Controls.Add(controles, 0, 0);
            Controls.Add(tres, 0, 1);
            Controls.Add(_lblEstado, 0, 2);

            Refrescar();
        }

        private static GroupBox CrearGrupo(string titulo, out DataGridView grid, out Label lblCount, out Label lblError)
        {
            var grupo = new GroupBox { Text = titulo, Dock = DockStyle.Fill, Padding = new Padding(6) };
            var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));

            lblCount = new Label { AutoSize = true, Margin = new Padding(3, 3, 3, 3) };
            lblCount.Font = new Font(lblCount.Font, FontStyle.Bold);
            grid = UiHelper.CrearGrid("Valor");
            lblError = new Label { Dock = DockStyle.Fill, AutoSize = false };

            t.Controls.Add(lblCount, 0, 0);
            t.Controls.Add(grid, 0, 1);
            t.Controls.Add(lblError, 0, 2);
            grupo.Controls.Add(t);
            return grupo;
        }

        public void Refrescar()
        {
            Dibujar(_estatica, _gridEst, _countEst, true);
            Dibujar(_dinamica, _gridDin, _countDin, false);
            Dibujar(_nativa, _gridNat, _countNat, false);
        }

        private static void Dibujar(Pila<int> pila, DataGridView grid, Label lblCount, bool mostrarLibres)
        {
            UiHelper.LlenarGrid(grid, pila.ObtenerCopia(), mostrarLibres ? pila.Capacidad : 0,
                                v => new[] { v.ToString() }, 1);
            lblCount.Text = mostrarLibres
                ? $"Count = {pila.Count}   (top = {pila.Top}, tamaño = {pila.Capacidad})"
                : $"Count = {pila.Count}";
        }

        /// <summary>Aplica la operación a las tres pilas y reporta cuál falló y por qué.</summary>
        private void EnLasTres(string operacion, Func<Pila<int>, string> accion)
        {
            var objetivos = new (string Nombre, Pila<int> Pila, Label Err)[]
            {
                ("Estática", _estatica, _errEst),
                ("Dinámica", _dinamica, _errDin),
                ("Nativa", _nativa, _errNat)
            };

            var partes = new System.Collections.Generic.List<string>();
            bool hayError = false;

            foreach (var o in objetivos)
            {
                try
                {
                    string r = accion(o.Pila);
                    o.Err.ForeColor = Color.DarkGreen;
                    o.Err.Text = "OK: " + r;
                    partes.Add($"{o.Nombre}: OK ({r})");
                }
                catch (InvalidOperationException ex)       // Overflow / Underflow
                {
                    hayError = true;
                    o.Err.ForeColor = Color.Red;
                    o.Err.Text = "FALLÓ: " + ex.Message;
                    partes.Add($"{o.Nombre}: FALLÓ -> {ex.Message}");
                }
            }

            Refrescar();
            UiHelper.MostrarEstado(_lblEstado, $"{operacion} en las 3:  " + string.Join("   |   ", partes), hayError);
        }

        private void OnPush()
        {
            int v = (int)_numValor.Value;
            EnLasTres("Push(" + v + ")", p => { p.Push(v); return $"Push({v})"; });
            if (_numValor.Value < _numValor.Maximum) _numValor.Value += 1;   // siguiente valor para la demo
        }

        private void OnPop() => EnLasTres("Pop()", p => $"Pop() = {p.Pop()}");

        private void OnClear() => EnLasTres("Clear()", p => { p.Clear(); return "Clear()"; });

        private void OnReiniciar()
        {
            _estatica = new Pila<int>((int)_numTam.Value);
            _dinamica = new Pila<int>();
            _nativa = Pila<int>.CrearNativa();
            _errEst.Text = _errDin.Text = _errNat.Text = "";
            Refrescar();
            UiHelper.MostrarEstado(_lblEstado, $"Las 3 pilas se reiniciaron (estática de tamaño {_estatica.Capacidad}).", false);
        }
    }
}
