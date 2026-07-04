import { useCallback, useState } from 'react';

/** Tracks open/closed state plus an optional "entity being edited" for create/edit modals. */
export function useModalState<T = unknown>() {
  const [isOpen, setIsOpen] = useState(false);
  const [editingEntity, setEditingEntity] = useState<T | null>(null);

  const openCreateModal = useCallback(() => {
    setEditingEntity(null);
    setIsOpen(true);
  }, []);

  const openEditModal = useCallback((entity: T) => {
    setEditingEntity(entity);
    setIsOpen(true);
  }, []);

  const closeModal = useCallback(() => {
    setIsOpen(false);
    setEditingEntity(null);
  }, []);

  return {
    isOpen,
    editingEntity,
    isEditing: editingEntity !== null,
    openCreateModal,
    openEditModal,
    closeModal,
  };
}
