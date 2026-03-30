import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { of } from 'rxjs';
import { TodosComponent } from './todos.component';
import { AuthService } from '../../services/auth.service';
import { TodoService } from '../../services/todo.service';

describe('TodosComponent', () => {
  let component: TodosComponent;
  let fixture: ComponentFixture<TodosComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TodosComponent],
      providers: [
        {
          provide: AuthService,
          useValue: {
            user: signal({ email: 'user@example.com', displayName: 'User', expiresAt: '2099-01-01' }),
            logout: jasmine.createSpy('logout')
          }
        },
        {
          provide: TodoService,
          useValue: {
            getAll: () => of([]),
            create: () => of(),
            update: () => of(),
            toggle: () => of(),
            delete: () => of(void 0)
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TodosComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
