using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace WeSay.Data.Tests
{
	[TestFixture]
	public class InMemoryRecordListManagerTests
	{
		InMemoryRecordListManager<SimpleIntTestClass> _recordListManager;
		InMemoryRecordList<SimpleIntTestClass> _sourceRecords;

		[SetUp]
		public void Setup()
		{
			_sourceRecords = new InMemoryRecordList<SimpleIntTestClass>();
			_recordListManager = new InMemoryRecordListManager<SimpleIntTestClass>(_sourceRecords);
			for (int i = 0; i < 50; i++)
			{
				_sourceRecords.Add(new SimpleIntTestClass(i));
			}
		}

		[TearDown]
		public void TearDown()
		{
			_recordListManager.Dispose();
			_sourceRecords.Dispose();
		}

		[Test]
		public void Create()
		{
			Assert.IsNotNull(_recordListManager);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Create_NullSource_ThrowsException()
		{
			using (InMemoryRecordListManager<SimpleIntTestClass> recordListManager = new InMemoryRecordListManager<SimpleIntTestClass>(null))
			{
			}
		}

		[Test]
		public void Get_NoFilter_GivesAllRecords()
		{
			IRecordList<SimpleIntTestClass> data = _recordListManager.Get();
			Assert.IsNotNull(data);
			Assert.AreEqual(_sourceRecords, data);
		}

		[Test]
		public void Get_Filter_GivesFilterdRecords()
		{
			IRecordList<SimpleIntTestClass> data = _recordListManager.Get(new SimpleIntFilter(11, 20));
			Assert.IsNotNull(data);
			Assert.AreEqual(10, data.Count);
			Assert.AreEqual(11, ((SimpleIntTestClass)data[0]).I);
			Assert.AreNotEqual(_sourceRecords, data);
		}

		[Test]
		public void Get_SecondTime_GivesSameRecordList()
		{
			IRecordList<SimpleIntTestClass> recordList = _recordListManager.Get();
			Assert.AreSame(recordList, _recordListManager.Get());
		}

		[Test]
		public void Get_SameFilter_GivesSameRecordList()
		{
			SimpleIntFilter intFilter = new SimpleIntFilter(11, 20);
			IRecordList<SimpleIntTestClass> recordList = _recordListManager.Get(intFilter);
			Assert.AreSame(recordList, _recordListManager.Get(intFilter));
		}

		[Test]
		public void Get_DifferentFilter_GivesDifferentRecordList()
		{
			IRecordList<SimpleIntTestClass> recordList = _recordListManager.Get(new SimpleIntFilter(11, 20));
			Assert.AreNotSame(recordList, _recordListManager.Get(new SimpleIntFilter(11, 12)));
		}

		[Test]
		public void ChangeRecord_NoLongerMeetsFilterCriteria_RemovedFromRecordList_StillInMaster()
		{
			IRecordList<SimpleIntTestClass> recordList11to20 = _recordListManager.Get(new SimpleIntFilter(11, 20));
			Assert.AreEqual(10, recordList11to20.Count);
			recordList11to20[0].I = 10;
			Assert.AreEqual(9, recordList11to20.Count);
			Assert.AreEqual(12, recordList11to20[0].I);
			Assert.AreEqual(50, _sourceRecords.Count);
		}

		[Test]
		public void ChangeRecord_NoLongerMeetsFilterCriteria_RemovedFromSimilarRecordList()
		{
			IRecordList<SimpleIntTestClass> recordList11to20 = _recordListManager.Get(new SimpleIntFilter(11, 20));
			IRecordList<SimpleIntTestClass> recordList11to17 = _recordListManager.Get(new SimpleIntFilter(11, 17));
			recordList11to20[0].I = 10;
			Assert.AreEqual(6, recordList11to17.Count);
			Assert.AreEqual(12, recordList11to17[0].I);
			Assert.AreEqual(12, recordList11to20[0].I);
		}

		[Test]
		public void ChangeRecord_MeetsFilterCriteria_RemainsInSimilarRecordList()
		{
			IRecordList<SimpleIntTestClass> recordList11to20 = _recordListManager.Get(new SimpleIntFilter(11, 20));
			IRecordList<SimpleIntTestClass> recordList11to17 = _recordListManager.Get(new SimpleIntFilter(11, 17));
			recordList11to20[0].I = 12;
			Assert.AreEqual(7, recordList11to17.Count);
			Assert.AreEqual(10, recordList11to20.Count);
			Assert.AreEqual(12, recordList11to17[0].I);
			Assert.AreEqual(12, recordList11to20[0].I);
		}

		[Test]
		public void AddRecordToFiltered_AddedToMaster()
		{
			IRecordList<SimpleIntTestClass> recordList11to20 = _recordListManager.Get(new SimpleIntFilter(11, 20));
			recordList11to20.Add(new SimpleIntTestClass(15));
			Assert.AreEqual(51, _sourceRecords.Count);
		}

		[Test]
		public void AddRecordToFiltered_AddedToRelevantRecordLists()
		{
			IRecordList<SimpleIntTestClass> recordList11to20 = _recordListManager.Get(new SimpleIntFilter(11, 20));
			IRecordList<SimpleIntTestClass> recordList11to17 = _recordListManager.Get(new SimpleIntFilter(11, 17));
			recordList11to20.Add(new SimpleIntTestClass(15));
			Assert.AreEqual(8, recordList11to17.Count);
			Assert.AreEqual(11, recordList11to20.Count);
		}

		[Test]
		public void AddRecordToMaster_AddedToRelevantRecordLists()
		{
			IRecordList<SimpleIntTestClass> recordList11to20 = _recordListManager.Get(new SimpleIntFilter(11, 20));
			IRecordList<SimpleIntTestClass> recordList11to17 = _recordListManager.Get(new SimpleIntFilter(11, 17));
			_sourceRecords.Add(new SimpleIntTestClass(15));
			Assert.AreEqual(8, recordList11to17.Count);
			Assert.AreEqual(11, recordList11to20.Count);
		}

		[Test]
		public void AddRecordToFiltered_DoesNotMeetFilterCriteria_AddedToMasterOnly()
		{
			IRecordList<SimpleIntTestClass> recordList11to20 = _recordListManager.Get(new SimpleIntFilter(11, 20));
			recordList11to20.Add(new SimpleIntTestClass(50));
			Assert.AreEqual(10, recordList11to20.Count);
			Assert.AreEqual(51, _sourceRecords.Count);
		}

		[Test]
		public void AddRecordToFiltered_AlreadyExists_NotAdded()
		{
			IRecordList<SimpleIntTestClass> recordList11to20 = _recordListManager.Get(new SimpleIntFilter(11, 20));
			recordList11to20.Add(_sourceRecords[12]);
			Assert.AreEqual(10, recordList11to20.Count);
			Assert.AreEqual(50, _sourceRecords.Count);
		}

		[Test]
		public void RemoveRecordFromMaster_RemovedFromFilteredRecordLists()
		{
			IRecordList<SimpleIntTestClass> recordList10to19 = _recordListManager.Get(new SimpleIntFilter(10, 19));
			IRecordList<SimpleIntTestClass> recordList11to17 = _recordListManager.Get(new SimpleIntFilter(11, 17));
			_sourceRecords.Remove(recordList10to19[1]);
			Assert.AreEqual(49, _sourceRecords.Count);
			Assert.AreEqual(6, recordList11to17.Count);
			Assert.AreEqual(12, recordList11to17[0]);
			Assert.AreEqual(9, recordList10to19.Count);
			Assert.AreEqual(12, recordList10to19[1]);
		}

		[Test]
		public void RemoveRecordFromFiltered_RemovedFromFilteredRecordListsAndMaster()
		{
			IRecordList<SimpleIntTestClass> recordList10to19 = _recordListManager.Get(new SimpleIntFilter(10, 19));
			IRecordList<SimpleIntTestClass> recordList11to17 = _recordListManager.Get(new SimpleIntFilter(11, 17));
			recordList10to19.Remove(recordList10to19[1]);
			Assert.AreEqual(6, recordList11to17.Count);
			Assert.AreEqual(12, recordList11to17[0]);
			Assert.AreEqual(9, recordList10to19.Count);
			Assert.AreEqual(12, recordList10to19[1]);
			Assert.AreEqual(49, _sourceRecords.Count);
			Assert.AreEqual(12, _sourceRecords[10]);
		}

	}
}
