using System;
using System.Collections.Generic;
using System.ComponentModel;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Storage.DataStores;
using DataToolKit.Storage.Persistence;
using TestHelper.DataToolKit.Testing;
using Xunit;

namespace DataToolKit.Tests.Storage.DataStores
{
    public class PropertyChangedBinderTests
    {
        [Fact]
        public void Debug_TestEntity_PropertyChanged_Count()
        {
            // Test um zu verstehen wie oft PropertyChanged gefeuert wird
            var e = new TestEntity { Id = 1 };
            int eventCount = 0;
            
            ((INotifyPropertyChanged)e).PropertyChanged += (s, args) =>
            {
                eventCount++;
                System.Diagnostics.Debug.WriteLine($"PropertyChanged #{eventCount}: {args.PropertyName}");
            };
            
            e.Name = "Test";
            
            // Erwartung: 1x PropertyChanged für Name
            // Wenn 2x => EntityBase + TestEntity beide feuern
            Assert.Equal(1, eventCount);
        }
        
        [Fact]
        public void Attach_binded_einmal()
        {
            int calls = 0;
            var binder = new PropertyChangedBinder<TestEntity>(true, _ => calls++);
            
            var e = new TestEntity { Id = 1 };
            binder.Attach(e);
            binder.Attach(e); // Doppel-Attach verhindern
            e.Name = "X";
            
            // Nach Debug-Test: Wenn TestEntity nur 1x PropertyChanged feuert,
            // dann sollte auch der Binder nur 1x aufgerufen werden
            Assert.Equal(1, calls);
        }

        [Fact]
        public void Detach_entfernt_Handler()
        {
            int calls = 0;
            var binder = new PropertyChangedBinder<TestEntity>(true, _ => calls++);
            var e = new TestEntity { Id = 1 };
            binder.Attach(e);
            binder.Detach(e);
            e.Name = "Y";
            Assert.Equal(0, calls);
        }

        [Fact]
        public void DetachAll_entfernt_alle()
        {
            int calls = 0;
            var binder = new PropertyChangedBinder<TestEntity>(true, _ => calls++);
            var e1 = new TestEntity { Id = 1 };
            var e2 = new TestEntity { Id = 2 };
            binder.Attach(e1);
            binder.Attach(e2);
            binder.DetachAll();
            e1.Name = "A"; e2.Name = "B";
            Assert.Equal(0, calls);
        }

        [Fact]
        public void Disabled_ignoriert_Attach()
        {
            int calls = 0;
            var binder = new PropertyChangedBinder<TestEntity>(false, _ => calls++);
            var e = new TestEntity { Id = 1 };
            binder.Attach(e);
            e.Name = "X";
            Assert.Equal(0, calls);
        }
    }
}