using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.IO
{
	public abstract class DataFileBinary : DataFile
	{
		public sealed override FileFormats FileFormat => FileFormats.Binary;

		/// <summary>
		/// Returns a BinaryWriter with the file's specified write mode.
		/// </summary>
		/// <returns>New BinaryWriter for this file.</returns>
		public BinaryWriter GetWriter()
		{
			if (WriteMode == WriteModes.ReadOnly) return null;

			else
			{
				//append or overwrite?
				bool append = !ShouldOverwrite(WriteMode, TimesOpened);
				FileMode fileMode = append ? FileMode.Append : FileMode.Create;

				//increment filename?
				IncrementFilename();

				//open and return writer
				Stream stream = File.Open(Path, fileMode);
				TimesOpened++;
				return new BinaryWriter(stream);
			}
		} 

		/// <summary>
		/// Returns a BinaryReader for the file.
		/// </summary>
		/// <returns>New BinaryReader for this file.</returns>
		public BinaryReader GetReader()
		{
			Stream stream = File.Open(Path, FileMode.Open);
			return new BinaryReader(stream);
		}

		/// <summary>
		/// Returns an array of all bytes in the file.
		/// </summary>
		/// <returns>Array of bytes.</returns>
		public byte[] GetBytes()
		{
			return File.ReadAllBytes(Path);
		}

		/// <summary>
		/// Writes the given bytes to the file.
		/// </summary>
		/// <param name="bytes">Data to write.</param>
		/// /// <param name="flush">Optional: flush stream after writing.</param>
		public virtual void Write(byte[] bytes, bool flush = true)
		{
			using(BinaryWriter binaryWriter = GetWriter())
			{
				if (binaryWriter == null) return;

				binaryWriter.Write(bytes);

				if (flush) binaryWriter.Flush();
			}
		}
	}
}