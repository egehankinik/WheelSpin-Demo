using System;
using NUnit.Framework;
using WheelOfFortune.Core.Events;

namespace WheelOfFortune.Tests
{
    public class EventBusTests
    {
        #region Setup

        private readonly struct TestEvent : IGameEvent
        {
            public TestEvent(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        private readonly struct OtherEvent : IGameEvent
        {
        }

        private EventBus _bus;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
        }

        #endregion

        #region Tests

        [Test]
        public void Publish_DeliversEventToSubscriber()
        {
            var received = 0;
            _bus.Subscribe<TestEvent>(e => received = e.Value);

            _bus.Publish(new TestEvent(7));

            Assert.AreEqual(7, received);
        }

        [Test]
        public void Publish_DeliversToEverySubscriber()
        {
            var calls = 0;
            _bus.Subscribe<TestEvent>(_ => calls++);
            _bus.Subscribe<TestEvent>(_ => calls++);

            _bus.Publish(new TestEvent(1));

            Assert.AreEqual(2, calls);
        }

        [Test]
        public void Unsubscribe_StopsDelivery()
        {
            var calls = 0;
            Action<TestEvent> handler = _ => calls++;
            _bus.Subscribe(handler);
            _bus.Unsubscribe(handler);

            _bus.Publish(new TestEvent(1));

            Assert.AreEqual(0, calls);
        }

        [Test]
        public void Publish_OnlyReachesSubscribersOfThatEventType()
        {
            var calls = 0;
            _bus.Subscribe<OtherEvent>(_ => calls++);

            _bus.Publish(new TestEvent(1));

            Assert.AreEqual(0, calls);
        }

        [Test]
        public void Publish_WithoutSubscribers_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _bus.Publish(new TestEvent(1)));
        }

        [Test]
        public void Unsubscribe_DuringPublish_StillFinishesCurrentDelivery()
        {
            var calls = 0;
            Action<TestEvent> second = _ => calls++;
            _bus.Subscribe<TestEvent>(_ => _bus.Unsubscribe(second));
            _bus.Subscribe(second);

            _bus.Publish(new TestEvent(1));
            _bus.Publish(new TestEvent(2));

            Assert.AreEqual(1, calls);
        }

        #endregion
    }
}
