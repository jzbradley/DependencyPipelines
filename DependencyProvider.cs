using System;
using System.Collections.Generic;

public class DependencyProvider
{
    private readonly DependencyProvider _parent;

    private DependencyProvider(DependencyProvider parent = null)
    {
        _parent = parent;
    }

    public bool TryGet<TInterface>(out TInterface @interface)
    {
        @interface = default;
        return (_factories.TryGetValue(typeof(TInterface), out var factory)
                && factory.Get(out @interface))
               || (_parent!=null && _parent.TryGet(out @interface));
    }

    public TInterface Require<TInterface>() =>
        TryGet(out TInterface result) ? result : throw new MissingRequiredDependencyException<TInterface>();

    private readonly Dictionary<Type, Factory> _factories
        = new Dictionary<Type, Factory>();

    public class Builder
    {
        private readonly DependencyProvider _product;
        private bool _built;

        public Builder(DependencyProvider parent=null) => _product = new DependencyProvider(parent);

        public DependencyProvider Build()
        {
            ThrowIfBuilt();
            _built = true;
            return _product;
        }

        private void ThrowIfBuilt()
        {
            if (_built) throw new InvalidOperationException("Object has already been built.");
        }

        public Builder Add<TInterface>(Func<DependencyProvider,TInterface> implementationFactory)
        {
            ThrowIfBuilt();
            _product._factories.Add(typeof(TInterface), new Factory<TInterface>(()=>implementationFactory(_product)));
            return this;
        }
        public Builder Add<TInterface>(Func<TInterface> implementationFactory)
        {
            ThrowIfBuilt();
            _product._factories.Add(typeof(TInterface), new Factory<TInterface>(implementationFactory));
            return this;
        }

        public Builder Add<TInterface, TImplementation>(TImplementation implementation)
            where TImplementation : TInterface, new()
        {
            ThrowIfBuilt();
            _product._factories.Add(typeof(TInterface), new Factory<TInterface>(implementation));
            return this;
        }
    }

    abstract class Factory
    {
        public abstract Type Type { get; }
        protected abstract object Get();
        public bool Get<T>(out T result)
        {
            result = default;
            if (!typeof(T).IsAssignableFrom(Type)) return false;
            result = (T)Get();
            return true;
        }
    }

    class Factory<T> : Factory
    {
        private readonly Func<T> _method;
        public Factory(Func<T> method) => _method = method;
        public Factory(T instance) => _method = () => instance;
        public override Type Type => typeof(T);
        protected override object Get() => _method();
    }
}

