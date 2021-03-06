///////////////////////////////////////////////////////////
//  TinyExcelReader.cs
//  Implementation of the Class TinyExcelReader
//  Generated by Enterprise Architect
//  Created on:      06-5��-2018 13:40:28
//  Original author: drago
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using NPOI.SS.UserModel;

namespace FD.TinyEtl {
    public class TinyExcelReader<T> : TinyReader, IDisposable, IEnumerable<T>
    {
        private IWorkbook _workbook;

        private bool _closeStreamOnDispose = false;
        private Lazy<IEnumerable<T>> _enumerator = null;
        private bool _clearFields = false;
        private bool _isDisposed = false;


        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }//end TinyExcelReader

}//end namespace FD.TinyEtl