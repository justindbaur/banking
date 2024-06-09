import { AsyncPipe, CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, Inject, Input } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormControl,
  FormsModule,
} from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Observable, map, switchMap } from 'rxjs';
import { BASE_URL } from '../../app.module';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

type CustomFormEntry = CustomFormInput | CustomFormGroup;
type CustomFormInput = { type: 'input'; id: string; description: string };
type CustomFormGroup = {
  type: 'group';
  id: string;
  entries: CustomFormEntry[];
};

export const convertSchemaToForm = (
  schema: unknown,
  parent: string
): CustomFormEntry => {
  if (schema == null) {
    throw new Error('A schema object must be non-null');
  }

  if (typeof schema !== 'object') {
    throw new Error('A schema object should be an object');
  }

  if (!('type' in schema) || typeof schema.type !== 'string') {
    throw new Error('A schema must have a type property of type string');
  }

  switch (schema.type) {
    case 'string':
      return {
        type: 'input',
        id: parent,
        description: (schema as any).description,
      };
    case 'object': {
      // Validate
      if (
        !('properties' in schema) ||
        typeof schema.properties !== 'object' ||
        schema.properties == null ||
        Array.isArray(schema.properties)
      ) {
        throw new Error(
          'A type object must have a properties property that is dict like.'
        );
      }

      // Get a control for every property
      const controls = Object.entries(schema.properties).map(
        ([name, subSchema]) => {
          return convertSchemaToForm(subSchema, name);
        }
      );

      return { type: 'group', id: parent, entries: controls };
    }
    default:
      throw new Error(`Unsupported schema type: ${schema.type}`);
  }
};

export type StartToken = {
  requirementsSchema: unknown;
  state: string | null;
};

@Component({
  selector: 'app-form-element',
  standalone: true,
  imports: [CommonModule, MatInputModule, FormsModule],
  templateUrl: './form-element.html',
})
export class FormElement {
  @Input({ required: true, alias: 'entry' })
  entry: CustomFormEntry = null!;
}

@Component({
  standalone: true,
  template: `<form #dynForm (ngSubmit)="submit(dynForm)">
    <ng-container *ngIf="start$ | async as start">
      <app-form-element [entry]="start.form"></app-form-element>
      <input type="hidden" value="{{ start.state }}" />
      <button mat-raised-button color="primary" type="submit">Next</button>
    </ng-container>
  </form>`,
  imports: [AsyncPipe, CommonModule, FormElement, MatButtonModule],
})
export class NewItemComponent {
  start$: Observable<{ state: string | null; form: CustomFormEntry }>;

  constructor(
    route: ActivatedRoute,
    private readonly httpClient: HttpClient,
    @Inject(BASE_URL) private readonly baseUrl: string
  ) {
    const sourceId = route.paramMap.pipe(map((p) => p.get('id')!));

    this.start$ = sourceId.pipe(
      switchMap((id) => {
        return this.httpClient.get<StartToken>(
          `${this.baseUrl}/sources/${id}/start`
        );
      }),
      map((startToken) => {
        // Parse requirements
        return {
          state: startToken.state,
          form: convertSchemaToForm(startToken.requirementsSchema, '__root'),
        };
      })
    );
  }

  submit(s: any) {
    console.log('form', s);
  }
}
