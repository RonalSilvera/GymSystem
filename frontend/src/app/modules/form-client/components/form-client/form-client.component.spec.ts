import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { FormClientComponent } from './form-client.component';
import { ServicesService } from '../../../../core/services/services.service';

describe('FormClientComponent', () => {
  let component: FormClientComponent;
  let fixture: ComponentFixture<FormClientComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FormClientComponent],
      providers: [{ provide: ServicesService, useValue: { getMembershipTypes: () => of([]) } }]
    }).compileComponents();

    fixture = TestBed.createComponent(FormClientComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
