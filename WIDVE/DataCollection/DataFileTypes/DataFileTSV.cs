using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;

namespace WIDVE.DataCollection
{
	[CreateAssetMenu(fileName = nameof(DataFileTSV), menuName = nameof(DataFile) + "/" + nameof(DataFileTSV), order = WIDVEEditor.C_ORDER)]
	public class DataFileTSV : DataFileCSV
	{
		public override string Extension => "tsv";
		public override char Separator => '\t';

		//everything else is the same as CSV...
	}
}