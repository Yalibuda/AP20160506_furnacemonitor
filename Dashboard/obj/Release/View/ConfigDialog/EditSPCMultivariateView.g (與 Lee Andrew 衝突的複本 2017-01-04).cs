﻿#pragma checksum "..\..\..\..\View\ConfigDialog\EditSPCMultivariateView.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "3C3DBA46C45B884EBDF658A3C83C36FE"
//------------------------------------------------------------------------------
// <auto-generated>
//     這段程式碼是由工具產生的。
//     執行階段版本:4.0.30319.42000
//
//     對這個檔案所做的變更可能會造成錯誤的行為，而且如果重新產生程式碼，
//     變更將會遺失。
// </auto-generated>
//------------------------------------------------------------------------------

using Dashboard.ViewModel.ConfigDialog;
using Dashboard.ViewModel.Converter;
using Dashboard.ViewModel.Validation;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Dashboard.View.ConfigDialog {
    
    
    /// <summary>
    /// EditSPCMultivariateView
    /// </summary>
    public partial class EditSPCMultivariateView : System.Windows.Controls.Page, System.Windows.Markup.IComponentConnector {
        
        
        #line 125 "..\..\..\..\View\ConfigDialog\EditSPCMultivariateView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid MeanVect;
        
        #line default
        #line hidden
        
        
        #line 129 "..\..\..\..\View\ConfigDialog\EditSPCMultivariateView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid Covariance;
        
        #line default
        #line hidden
        
        
        #line 176 "..\..\..\..\View\ConfigDialog\EditSPCMultivariateView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid MeanVectEdit;
        
        #line default
        #line hidden
        
        
        #line 179 "..\..\..\..\View\ConfigDialog\EditSPCMultivariateView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid CovarianceEdit;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Dashboard;component/view/configdialog/editspcmultivariateview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\View\ConfigDialog\EditSPCMultivariateView.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.MeanVect = ((System.Windows.Controls.DataGrid)(target));
            return;
            case 2:
            this.Covariance = ((System.Windows.Controls.DataGrid)(target));
            return;
            case 3:
            this.MeanVectEdit = ((System.Windows.Controls.DataGrid)(target));
            return;
            case 4:
            this.CovarianceEdit = ((System.Windows.Controls.DataGrid)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

