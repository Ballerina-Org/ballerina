import React from "react";
import {
  PredicateValueRegistry,
  RegistryValue,
} from "../../../built-ins/state";
import { PredicateValue } from "../../../../parser/domains/predicates/state";

type Listener = () => void;

class RegistryStore {
  private registry: PredicateValueRegistry;
  private listeners: Set<Listener> = new Set();

  constructor(initial: PredicateValueRegistry) {
    this.registry = initial;
  }

  getSnapshot(): PredicateValueRegistry {
    return this.registry;
  }

  set(next: PredicateValueRegistry): void {
    this.registry = next;
    this.listeners.forEach((l) => l());
  }

  subscribe(listener: Listener): () => void {
    this.listeners.add(listener);
    return () => {
      this.listeners.delete(listener);
    };
  }
}

const RegistryStoreContext = React.createContext<RegistryStore | null>(null);

export const createRegistryStore = (
  initial: PredicateValueRegistry,
): RegistryStore => new RegistryStore(initial);

export const RegistryStoreProvider = (props: {
  store: RegistryStore;
  children?: React.ReactNode;
}) => (
  <RegistryStoreContext.Provider value={props.store}>
    {props.children}
  </RegistryStoreContext.Provider>
);

export const useRegistrySliceSignature = (path: string): bigint | undefined => {
  const store = React.useContext(RegistryStoreContext);
  // If no provider/store available, behave as undefined (no re-render on registry changes)
  if (!store) return undefined;
  return React.useSyncExternalStore(
    store.subscribe.bind(store),
    () => store.getSnapshot().get(path)?.signature,
    () => store.getSnapshot().get(path)?.signature,
  );
};

export const useRegistrySlice = (path: string): RegistryValue | undefined => {
  const store = React.useContext(RegistryStoreContext);
  if (!store) return undefined;
  // Use signature as the snapshot to gate re-renders, then read full value
  React.useSyncExternalStore(
    store.subscribe.bind(store),
    () => store.getSnapshot().get(path)?.signature,
    () => store.getSnapshot().get(path)?.signature,
  );
  return store.getSnapshot().get(path);
};

export const useRegistryValueAtPath = (
  path: string,
): PredicateValue | undefined => {
  const slice = useRegistrySlice(path);
  return slice?.value;
};
