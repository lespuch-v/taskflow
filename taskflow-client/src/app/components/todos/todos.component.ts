import { CommonModule, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { TodoItem, UpdateTodoItem } from '../../models/todo.model';
import { AuthService } from '../../services/auth.service';
import { TodoService } from '../../services/todo.service';

@Component({
  selector: 'app-todos',
  imports: [CommonModule, ReactiveFormsModule, DatePipe],
  templateUrl: './todos.component.html',
  styleUrl: './todos.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TodosComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly todoService = inject(TodoService);

  readonly todos = signal<TodoItem[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal('');
  readonly createPending = signal(false);
  readonly actionTodoId = signal<number | null>(null);
  readonly editingId = signal<number | null>(null);

  readonly createForm = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(1000)]]
  });

  readonly editForm = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(1000)]]
  });

  readonly user = this.auth.user;
  readonly remainingCount = computed(() => this.todos().filter(item => !item.isCompleted).length);
  readonly completedCount = computed(() => this.todos().filter(item => item.isCompleted).length);

  constructor() {
    this.loadTodos();
  }

  createTodo() {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    const dto = {
      title: this.createForm.controls.title.getRawValue().trim(),
      description: this.normalizeDescription(this.createForm.controls.description.getRawValue())
    };

    this.createPending.set(true);
    this.errorMessage.set('');

    this.todoService.create(dto)
      .pipe(finalize(() => this.createPending.set(false)))
      .subscribe({
        next: todo => {
          this.todos.update(items => [todo, ...items]);
          this.createForm.reset({ title: '', description: '' });
        },
        error: error => {
          this.errorMessage.set(error.error?.message ?? 'Unable to create todo.');
        }
      });
  }

  startEdit(todo: TodoItem) {
    this.editingId.set(todo.id);
    this.editForm.reset({
      title: todo.title,
      description: todo.description ?? ''
    });
    this.errorMessage.set('');
  }

  cancelEdit() {
    this.editingId.set(null);
  }

  saveEdit(todo: TodoItem) {
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    const update: UpdateTodoItem = {
      title: this.editForm.controls.title.getRawValue().trim(),
      description: this.normalizeDescription(this.editForm.controls.description.getRawValue()),
      isCompleted: todo.isCompleted
    };

    this.actionTodoId.set(todo.id);
    this.errorMessage.set('');

    this.todoService.update(todo.id, update)
      .pipe(finalize(() => this.actionTodoId.set(null)))
      .subscribe({
        next: updated => {
          this.replaceTodo(updated);
          this.editingId.set(null);
        },
        error: error => {
          this.errorMessage.set(error.error?.message ?? 'Unable to save todo.');
        }
      });
  }

  toggleTodo(todo: TodoItem) {
    this.actionTodoId.set(todo.id);
    this.errorMessage.set('');

    this.todoService.toggle(todo.id)
      .pipe(finalize(() => this.actionTodoId.set(null)))
      .subscribe({
        next: updated => this.replaceTodo(updated),
        error: error => {
          this.errorMessage.set(error.error?.message ?? 'Unable to update todo.');
        }
      });
  }

  deleteTodo(todo: TodoItem) {
    if (!window.confirm(`Delete "${todo.title}"?`)) {
      return;
    }

    this.actionTodoId.set(todo.id);
    this.errorMessage.set('');

    this.todoService.delete(todo.id)
      .pipe(finalize(() => this.actionTodoId.set(null)))
      .subscribe({
        next: () => {
          this.todos.update(items => items.filter(item => item.id !== todo.id));
          if (this.editingId() === todo.id) {
            this.editingId.set(null);
          }
        },
        error: error => {
          this.errorMessage.set(error.error?.message ?? 'Unable to delete todo.');
        }
      });
  }

  logout() {
    this.auth.logout();
  }

  isBusy(todoId: number) {
    return this.actionTodoId() === todoId;
  }

  private loadTodos() {
    this.loading.set(true);
    this.errorMessage.set('');

    this.todoService.getAll()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: todos => this.todos.set(todos),
        error: error => {
          this.errorMessage.set(error.error?.message ?? 'Unable to load todos.');
        }
      });
  }

  private replaceTodo(updated: TodoItem) {
    this.todos.update(items => items.map(item => item.id === updated.id ? updated : item));
  }

  private normalizeDescription(value: string) {
    const trimmed = value.trim();
    return trimmed ? trimmed : undefined;
  }
}
