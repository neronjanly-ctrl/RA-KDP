using RatCvsSimulation.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace RatCvsSimulation.Core
{
    /// <summary>
    /// Define .labrat file format and methods for accessing a .labrat file.
    /// A .labrat file contains a lots of consecutive strips, each stores information of 8 channels
    /// for plotting on time tick.
    /// </summary>
    public class Document : IDisposable, IEnumerable<CardiogramData>
    {
        #region constants
        /// <summary>
        /// A segment contains 64 strips for 64 ticks which is the basic scrolling amount
        /// </summary>
        public const uint SegmentLength = 64;
        /// <summary>
        /// A page contains 4096 strips which is the ticking length for drawing a screen page
        /// </summary>
        public const uint PageLength = 4096;
        /// <summary>
        /// Shift a quarter page upon fully drawn of screen
        /// </summary>
        public const uint RollingLength = PageLength / 4;

        /// <summary>
        /// The total bytes of a single cardiogram data structure
        /// </summary>
        public const uint CardiogramDataBytes = sizeof(float) * 5 + sizeof(int) + sizeof(float) + sizeof(UnitType);
        /// <summary>
        /// The total bytes of a segment
        /// </summary>
        public const uint SegmentBytes = SegmentLength * CardiogramDataBytes;
        /// <summary>
        /// The total bytes of a page
        /// </summary>
        public const uint PageBytes = PageLength * CardiogramDataBytes;
        /// <summary>
        /// The total bytes of a rolling amount
        /// </summary>
        public const uint RollingBytes = RollingLength * CardiogramDataBytes;
        #endregion

        #region properties
        /// <summary>
        /// The total bytes of all pushed data
        /// </summary>
        public ulong TotalBytes => _stream.Size;
        /// <summary>
        /// The total number of segments of all pushed data
        /// </summary>
        public ulong TotalSegments => (_stream.Size + SegmentBytes - 1) / SegmentBytes;
        /// <summary>
        /// The total number of pages of all pushed data
        /// </summary>
        public ulong TotalPages => (_stream.Size + PageBytes - 1) / PageBytes;
        /// <summary>
        /// The number of current available data on the page buffer
        /// </summary>
        public uint BufferLength => _dataCursor;
        #endregion

        /// <summary>
        /// Element accessor for visiting page buffer
        /// </summary>
        /// <param name="strip">Strip number within buffer range</param>
        /// <returns></returns>
        public CardiogramData this[uint strip] => _data[strip];

        /// <summary>
        /// The currently displayed data
        /// StripCount strips in total
        /// </summary>
        private CardiogramData[] _data = new CardiogramData[PageLength];

        /// <summary>
        /// Pointer position in strips of the memory buffer array
        /// The range is from 0 to StripCount - 1
        /// </summary>
        private uint _dataCursor = 0;

        /// <summary>
        /// Work file is randomly read when user scrolls historical data and is sequentially written when time ticks
        /// </summary>
        StorageFile _workfile;

        IRandomAccessStream _stream;
        DataWriter _writer;
        DataReader _reader;

        /// <summary>
        /// Append data to the page buffer, roll when it is full.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task PushStrip(ChannelSpecification<float> data)
        {
            Marker marker = _tempMarkerStack.Count > 0 ? _tempMarkerStack.Pop() : new Marker();

            var cd = new CardiogramData();
            cd.SetData(data, marker);

            // move cursor to the end of work file
            _stream.Seek(_stream.Size);

            _data[_dataCursor] = cd;
            _writer.WriteCardiogramData(cd);

            _dataCursor++; // starting from 0, increments for each strip

            // Store data into work file segment by segment
            if (_writer.UnstoredBufferLength >= SegmentBytes)
            {
                await _writer.StoreAsync();
            }

            // Roll when the data buffer is full
            if (_dataCursor >= PageLength)
            {
                Roll();
            }
        }

        Stack<Marker> _tempMarkerStack = new Stack<Marker>();
        public void PushMarker(int index, float value, UnitType unit)
        {
            _tempMarkerStack.Push(new Marker
            {
                Index = index,
                Value = value,
                Units = unit,
            });
        }

        /// <summary>
        /// Shift the page and discard part of the page from the beginning
        /// </summary>
        private void Roll()
        {
            for (uint i = 0, j = RollingLength; j < PageLength; i++, j++)
            {
                _data[i] = _data[j];
            }

            _dataCursor = PageLength - RollingLength;
        }

        /// <summary>
        /// Load a page from work file into memory buffer by segment position
        /// </summary>
        /// <param name="position">Position in segments starting from 0</param>
        /// <returns></returns>
        public async Task LoadPage(uint position)
        {
            // move the cursor to the position for reading
            _stream.Seek(SegmentBytes * position);

            await _reader.LoadAsync(PageBytes);

            uint i;
            for (i = 0; _reader.UnconsumedBufferLength > 0; i++)
            {
                _data[i] = _reader.ReadCardiogramData();
            }

            // save the new length
            _dataCursor = i;
        }

        /// <summary>
        /// Clear the page buffer
        /// </summary>
        public void ClearPage()
        {
            _dataCursor = 0;
        }

        /// <summary>
        /// Create an empty .labrat file in temporary folder and prepare input and output stream
        /// </summary>
        /// <returns></returns>
        public async Task NewFile()
        {
            // Get path to temporary file directory
            var tempFolder = ApplicationData.Current.TemporaryFolder;

            // Create a temp file name
            var workFileName = string.Format("{0}.labrat", ((int)DateTime.Now.Ticks).ToString("x8"));

            // Create an empty .labrat file
            _workfile = await tempFolder.CreateFileAsync(workFileName,
                CreationCollisionOption.ReplaceExisting);

            // Open work file
            _stream = await _workfile.OpenAsync(FileAccessMode.ReadWrite);

            if (_stream == null)
                throw new Exception("Cannot open work file!");

            _reader = new DataReader(_stream);
            _writer = new DataWriter(_stream);
        }

        /// <summary>
        /// Open the specified .labrat storage file
        /// </summary>
        /// <param name="file">A .labrat file to be read</param>
        /// <returns></returns>
        public async Task OpenFile(StorageFile file)
        {
            // Get path to temporary file directory
            var tempFolder = ApplicationData.Current.TemporaryFolder;

            // Create a temp file name
            var workFileName = string.Format("{0}.labrat", DateTime.Now.Ticks);

            // Copy data from .labrat storage file into work file
            _workfile = await file.CopyAsync(tempFolder, workFileName, NameCollisionOption.ReplaceExisting);

            // Open work file
            _stream = await _workfile.OpenAsync(FileAccessMode.ReadWrite);

            if (_stream == null)
                throw new Exception("Cannot open work file!");

            _reader = new DataReader(_stream);
            _writer = new DataWriter(_stream);
        }

        /// <summary>
        /// Save the unsaved changes to the specified storage file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task SaveFile(StorageFile file)
        {
            await _writer.StoreAsync();
            await _writer.FlushAsync();

            //  Copy data from work file into .labrat storage file
            await _workfile.CopyAndReplaceAsync(file);
        }

        public async Task Cleanup()
        {
            _reader.Dispose();
            _reader = null;

            _writer.Dispose();
            _writer = null;

            _stream.Dispose();
            _stream = null;

            await _workfile.DeleteAsync();
            _workfile = null;
        }

        public void Dispose()
        {
            Cleanup().Wait();
        }

        public IEnumerator<CardiogramData> GetEnumerator()
        {
            for (int i = 0; i < _dataCursor; i++)
            {
                yield return _data[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
