using cnData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace lsa
{
    public partial class SYS101L : frmBaseForm
    {
        private TViewModel_InspData _vmInspData;
        //private TViewModel_InspData _vmInspData;
        private readonly BindingSource _edtBindingSource = new BindingSource();

        public SYS101L()
        {
            InitializeComponent();

            this.Load += (sender, e) =>
            {
                _vmInspData = new TViewModel_InspData();
                setDataBinding();
            };

            this.Shown += (sender, e) =>
            {
                selectInspData();
            };

            this.FormClosing += (sender, e) =>
            {
                //
            };

            this.FormClosed += (sender, e) =>
            {
                //_inspSource.Dispose();
            };

            this.btnSelect.Click += (sender, e) =>
            {
                selectInspData();
            };

            this.btnInsert.Click += (sender, e) =>
            {
                //edtGet003.Text = string.Empty; // 발주번호
                //edtGet004.Text = "1";          // 발주순번
                _vmInspData.BeforeInsert();
                _edtBindingSource.DataSource = _vmInspData.EditItem;

                edtGet003.Text = string.Empty; // 발주번호
            };

            this.btnDelete.Click += (sender, e) =>
            {
                TModel_InspData vItem = grvData.CurrentRow?.DataBoundItem as TModel_InspData;
                if (vItem == null) return;
                _vmInspData.Delete(vItem);
            };

            this.btnUpdate.Click += (sender, e) =>
            {
                if (_vmInspData.IsInsert)
                {
                    TModel_InspData vItem = grvData.CurrentRow?.DataBoundItem as TModel_InspData;

                    if (vItem == null) return;

                    _vmInspData.Insert(vItem);
                }
                else
                {
                    TModel_InspData vItem = grvData.CurrentRow?.DataBoundItem as TModel_InspData;

                    if (vItem == null) return;

                    //_vmInspData.BeginUpdate(item);
                    _vmInspData.Update(vItem);
                }
            };
        }

        private void selectInspData()
        {
            DataTable vdatInspData = TDataMySql.GetDataTable("SELECT * FROM tb_insp_data");
            _vmInspData.ToList(vdatInspData);
            grvData.DataSource = _vmInspData.Items;
        }

        protected override void SetButtonEnabled()
        {
            //bool available = !_viewModelInsp.IsBusy;
            //btnSelect.Enabled = tsbSelect.Enabled = available;
            //btnInsert.Enabled = tsbInsert.Enabled = available;
            //btnUpdate.Enabled = tsbUpdate.Enabled = available && _viewModelInsp.EditItem != null;
            //btnDelete.Enabled = tsbDelete.Enabled = available && !_viewModelInsp.IsNew && _viewModelInsp.SelectedItem != null;
            //grvData.Enabled = available;
        }

        protected override void setDataBinding()
        {
            _edtBindingSource.DataSource = typeof(TModel_InspData);

            edtGet003.DataBindings.Clear();
            edtGet004.DataBindings.Clear();
            edtGet005.DataBindings.Clear();
            edtGet006.DataBindings.Clear();
            edtGet007.DataBindings.Clear();
            edtGet008.DataBindings.Clear();
            edtGet009.DataBindings.Clear();
            edtGet010.DataBindings.Clear();
            edtGet011.DataBindings.Clear();
            edtGet012.DataBindings.Clear();

            //edtGet003.DataBindings.Add(nameof(edtGet003.Text), _vmInspData.Items, nameof(TModel_InspData.OrderNo), true, DataSourceUpdateMode.OnPropertyChanged);
            //edtGet004.DataBindings.Add(nameof(edtGet004.Text), _vmInspData.Items, nameof(TModel_InspData.OrderSeq), true, DataSourceUpdateMode.OnPropertyChanged);
            //edtGet005.DataBindings.Add(nameof(edtGet004.Text), _vmInspData.Items, nameof(TModel_InspData.CustCode), true, DataSourceUpdateMode.OnPropertyChanged);
            //edtGet006.DataBindings.Add(nameof(edtGet004.Text), _vmInspData.Items, nameof(TModel_InspData.ItemNo), true, DataSourceUpdateMode.OnPropertyChanged);
            //edtGet007.DataBindings.Add(nameof(edtGet004.Text), _vmInspData.Items, nameof(TModel_InspData.Plant), true, DataSourceUpdateMode.OnPropertyChanged);
            //edtGet008.DataBindings.Add(nameof(edtGet004.Text), _vmInspData.Items, nameof(TModel_InspData.Inlet), true, DataSourceUpdateMode.OnPropertyChanged);
            //edtGet009.DataBindings.Add(nameof(edtGet004.Text), _vmInspData.Items, nameof(TModel_InspData.Outlet), true, DataSourceUpdateMode.OnPropertyChanged);
            //edtGet010.DataBindings.Add(nameof(edtGet004.Text), _vmInspData.Items, nameof(TModel_InspData.SetPress), true, DataSourceUpdateMode.OnPropertyChanged);
            //edtGet011.DataBindings.Add(nameof(edtGet004.Text), _vmInspData.Items, nameof(TModel_InspData.BackPress), true, DataSourceUpdateMode.OnPropertyChanged);

        }
    }
}
