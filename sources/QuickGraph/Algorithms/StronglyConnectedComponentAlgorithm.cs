﻿using System;
using System.Collections.Generic;

using QuickGraph.Algorithms.Search;

namespace QuickGraph.Algorithms
{
    [Serializable]
    public sealed class StronglyConnectedComponentsAlgorithm<TVertex, TEdge> :
        AlgorithmBase<IVertexListGraph<TVertex, TEdge>>,
        IConnectedComponentAlgorithm<TVertex,TEdge,IVertexListGraph<TVertex, TEdge>>
        where TEdge : IEdge<TVertex>
    {
		private IDictionary<TVertex,int> components;
		private IDictionary<TVertex,int> discoverTimes;
		private IDictionary<TVertex,TVertex> roots;
		private Stack<TVertex> stack;
		int componentCount;
		int dfsTime;
        private DepthFirstSearchAlgorithm<TVertex, TEdge> dfs;

        public StronglyConnectedComponentsAlgorithm(
            IVertexListGraph<TVertex,TEdge> g)
            :this(g, new Dictionary<TVertex,int>())
		{}

        public StronglyConnectedComponentsAlgorithm(
            IVertexListGraph<TVertex,TEdge> g,
			IDictionary<TVertex,int> components)
            :base(g)
		{
			if (components==null)
				throw new ArgumentNullException("components");

			this.components = components;
            this.roots = new Dictionary<TVertex, TVertex>();
            this.discoverTimes = new Dictionary<TVertex, int>();
            this.stack = new Stack<TVertex>();
			this.componentCount = 0;
			this.dfsTime = 0;
            this.dfs = new DepthFirstSearchAlgorithm<TVertex, TEdge>(VisitedGraph);
            this.dfs.DiscoverVertex += new VertexEventHandler<TVertex>(this.DiscoverVertex);
            this.dfs.FinishVertex += new VertexEventHandler<TVertex>(this.FinishVertex);
        }

		public IDictionary<TVertex,int> Components
		{
			get
			{
				return this.components;
			}
		}

		public IDictionary<TVertex,TVertex> Roots
		{
			get
			{
				return this.roots;
			}
		}

        public IDictionary<TVertex, int> DiscoverTimes
        {
			get
			{
				return this.discoverTimes;
			}
		}

		public int ComponentCount
		{
			get
			{
				return this.componentCount;
			}
		}

		private void DiscoverVertex(Object sender, VertexEventArgs<TVertex> args)
		{
			TVertex v = args.Vertex;
			this.Roots[v]=v;
			this.Components[v]=int.MaxValue;
			this.DiscoverTimes[v]=dfsTime++;
			this.stack.Push(v);
		}

		/// <summary>
		/// Used internally
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void FinishVertex(Object sender, VertexEventArgs<TVertex> args)
		{
			TVertex v = args.Vertex;
			foreach(TEdge e in VisitedGraph.OutEdges(v))
			{
				TVertex w = e.Target;
				if (this.Components[w] == int.MaxValue)
					this.Roots[v]=MinDiscoverTime(this.Roots[v], this.Roots[w]);
			}

			if (Roots[v].Equals(v)) 
			{
				TVertex w=default(TVertex);
				do 
				{
					w = this.stack.Pop(); 
					this.Components[w]=componentCount;
				} 
				while (!w.Equals(v));
				++componentCount;
			}	
		}

		private TVertex MinDiscoverTime(TVertex u, TVertex v)
		{
			if (this.DiscoverTimes[u]<this.DiscoverTimes[v])
				return u;
			else
				return v;
		}

        public override void Abort()
        {
            this.dfs.Abort();
            base.Abort();
        }

		protected override void InternalCompute()
		{
			this.Components.Clear();
			this.Roots.Clear();
			this.DiscoverTimes.Clear();
			componentCount = 0;
			dfsTime = 0;

			dfs.Compute();
		}
    }
}
