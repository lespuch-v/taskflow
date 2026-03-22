import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { TodoItem, CreateTodoItem, UpdateTodoItem } from '../models/todo.model';

@Injectable({ providedIn: 'root' })
export class TodoService {
    private readonly http = inject(HttpClient);
    private readonly base = `${environment.apiUrl}/todoitems`;

    getAll() { return this.http.get<TodoItem[]>(this.base); }
    getById(id: number) { return this.http.get<TodoItem>(`${this.base}/${id}`); }
    create(dto: CreateTodoItem) { return this.http.post<TodoItem>(this.base, dto); }
    update(id: number, dto: UpdateTodoItem) { return this.http.put<TodoItem>(`${this.base}/${id}`, dto); }
    toggle(id: number) { return this.http.patch<TodoItem>(`${this.base}/${id}/toggle`, {}); }
    delete(id: number) { return this.http.delete<void>(`${this.base}/${id}`); }
}