//  Author:
//       Noah Ablaseau <nablaseau@hotmail.com>
//
//  Copyright (c) 2017 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace linerider.Utils
{
    public sealed class ResourceSync
    {
        /// <summary>
        /// Creates a disposable object around a recently acquired lock
        /// </summary>
        public sealed class ResourceLock(
            bool read,
            bool write,
            bool upgradableread,
            ResourceSync parent) : IDisposable
        {
            private bool _disposed = false;
            private readonly bool _upgradableread = upgradableread;
            private readonly bool _read = read;
            private bool _write = write;
            private readonly ResourceSync _parent = parent;
            public bool WaitedOn
            {
                get
                {
                    ReaderWriterLockSlim l = _parent._lock;
                    return l.WaitingReadCount > 0 ||
                    l.WaitingUpgradeCount > 0 ||
                    l.WaitingWriteCount > 0;
                }
            }

            public static ResourceLock Reader(ResourceSync parent) => new(true, false, false, parent);
            public static ResourceLock Writer(ResourceSync parent) => new(false, true, false, parent);
            public static ResourceLock UpgradableReader(ResourceSync parent) => new(false, false, true, parent);
            public void UpgradeToWriter()
            {
                if (_disposed)
                    return;
                if (!_upgradableread)
                {
                    throw new InvalidOperationException("Attempt to upgrade a non upgradable resource reader");
                }
                if (_write)
                {
                    throw new InvalidOperationException("Attempt to upgrade an already upgraded resource reader");
                }
                _parent.UnsafeEnterWrite();
                _write = true;
            }
            /// <summary>
            /// Drops and reacquires the lock until nobody is waiting on us.
            /// </summary>
            public void ReleaseWaiting()
            {
                int count = 0;
                while (WaitedOn)
                {
                    Debug.Assert(count++ != 10000, "Wait release in possible infinite loop");
                    Release();
                    Acquire();
                }
            }
            private void Acquire()
            {
                if (_upgradableread)
                {
                    _parent.UnsafeEnterUpgradableRead();
                    if (_write)
                    {
                        _parent.UnsafeEnterWrite();
                    }
                }
                else if (_read)
                {
                    _parent.UnsafeEnterRead();
                }
                else if (_write)
                {
                    _parent.UnsafeEnterWrite();
                }
                else
                {
                    throw new Exception("Unknown resourcesync type");
                }
            }
            private void Release()
            {
                if (_upgradableread)
                {
                    _parent.UnsafeExitUpgradableRead();
                }
                else if (_read)
                {
                    _parent.UnsafeExitRead();
                }
                else if (_write)
                {
                    _parent.UnsafeExitWrite();
                }
                else
                {
                    throw new Exception("Unknown resourcesync type");
                }
            }
            public void Dispose()
            {
                if (_disposed)
                    return;
                Release();
                _disposed = true;
            }
        }
        private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);
        public ResourceLock TryAcquireRead() => _lock.TryEnterReadLock(0) ? ResourceLock.Reader(this) : null;
        public ResourceLock AcquireRead()
        {
            UnsafeEnterRead();
            return ResourceLock.Reader(this);
        }
        public ResourceLock AcquireUpgradableRead()
        {
            UnsafeEnterUpgradableRead();
            return ResourceLock.UpgradableReader(this);
        }
        public ResourceLock AcquireWrite()
        {
            UnsafeEnterWrite();
            return ResourceLock.Writer(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsafeEnterRead() => _lock.EnterReadLock();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsafeExitRead() => _lock.ExitReadLock();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsafeEnterWrite() => _lock.EnterWriteLock();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsafeExitWrite() => _lock.ExitWriteLock();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsafeEnterUpgradableRead() => _lock.EnterUpgradeableReadLock();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsafeExitUpgradableRead() => _lock.ExitUpgradeableReadLock();
    }
}
