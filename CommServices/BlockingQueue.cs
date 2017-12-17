////////////////////////////////////////////////////////////////////////////////////////
// BlockingQueue.cs:This package performs enqueueing and dequeueing of blocking queue.//
// ver 1.0                                                                            //
//                                                                                    //
//Language:     Visual C#                                                             //
// Platform    : Lenovo 510S Ideapad, Win Pro 10, Visual Studio 2017                  //
// Application : CSE-681 SMA Project 4                                                //
// Author      : Adarsh Venkatesh Bodineni,Syracuse University                        //
// Source      : Dr. Jim Fawcett, EECS, SU                                            //
////////////////////////////////////////////////////////////////////////////////////////

/*
* Module Operations:
*===================
 *   This package implements a generic blocking queue and demonstrates 
 *   communication between two threads using an instance of the queue. 
 *   If the queue is empty when a reader attempts to deQ an item the
 *   reader will block until the writing thread enQs an item.  Thus waiting
 *   is efficient.
*
* public Interfaces:
* =================
 *   BlockingQueue<string> bQ = new BlockingQueue<string>():Creates new instance of queue.
 *   enQ(msg):enqueues message into the queue.
 *   deQ(): Dequeues message from the queue.
 *   size(): returns the size of the blocking queue.
 *   clear(): Clears the blocking queue.
*
* Required Files:
* ===============
* BasicService.cs
*
* Maintainance History:
* =====================
* ver 1.0
*
*/

//
using System;
using System.Collections;
using System.Threading;

namespace SWTools
{
    public class BlockingQueue<T>
    {
        private Queue blockingQ;
        object locker_ = new object();

        //----< constructor >--------------------------------------------

        public BlockingQueue()
        {
            blockingQ = new Queue();
        }


        //----< enqueues a message >---------------------------------------

        public void enQ(T msg)
        {
            lock (locker_)  // uses Monitor
            {
                blockingQ.Enqueue(msg);
                Monitor.Pulse(locker_);
            }
        }


        //----< dequeue a message >---------------------------------------

        public T deQ()
        {
            T msg = default(T);
            lock (locker_)
            {
                while (this.size() == 0)
                {
                    Monitor.Wait(locker_);
                }
                msg = (T)blockingQ.Dequeue();
                return msg;
            }
        }


        //----< return number of elements in queue >---------------------

        public int size()
        {
            int count;
            lock (locker_) { count = blockingQ.Count; }
            return count;
        }


        //----< purge elements from queue >------------------------------

        public void clear()
        {
            lock (locker_) { blockingQ.Clear(); }
        }
    }
    public class test
    { 
#if (BLOCKINGQUEUE)
        static void Main(string[] args)
        {
            BlockingQueue<String> blockingQ = new BlockingQueue<String>();
            Console.Write("Enqueueing message into the blocking queue");
            blockingQ.enQ("message");
            Console.Write("Dequeueing message from the blocking queue");
            string msg = blockingQ.deQ();
            Console.Write("The message is:" + msg);
            Console.Read();
        }
#endif
    }
}

