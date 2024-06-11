import { Component } from '@angular/core';
import { SourceListItem, SourcesService } from './sources.service';
import { Observable, map } from 'rxjs';
import { AsyncPipe, CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-sources-list',
  standalone: true,
  template: `<table mat-table [dataSource]="sources$">
    <ng-container matColumnDef="displayName">
      <th mat-header-cell *matHeaderCellDef>Name</th>
      <td mat-cell *matCellDef="let element">
        {{ element.displayName ?? element.sourceId }}
      </td>
    </ng-container>

    <ng-container matColumnDef="enabled">
      <th mat-header-cell *matHeaderCellDef>Enabled</th>
      <td mat-cell *matCellDef="let element">
        {{ element.enabled ? 'Yes' : 'No' }}
      </td>
    </ng-container>

    <ng-container matColumnDef="star">
      <th mat-header-cell *matHeaderCellDef aria-label="row actions">&nbsp;</th>
      <td mat-cell *matCellDef="let element">
        <mat-icon>more_vert</mat-icon>
      </td>
    </ng-container>

    <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
    <tr
      mat-row
      *matRowDef="let row; columns: displayedColumns"
      (click)="rowClick(row)"
    ></tr>
  </table>`,
  imports: [AsyncPipe, CommonModule, RouterLink, MatTableModule, MatIconModule],
  providers: [SourcesService],
  styles: [''],
})
export class ListComponent {
  readonly displayedColumns = ['displayName', 'enabled', 'star'];

  sources$: Observable<SourceListItem[]>;

  constructor(
    private readonly sourcesService: SourcesService,
    private readonly router: Router
  ) {
    this.sources$ = this.sourcesService.getAll$().pipe(map((l) => l.data));
  }

  async rowClick(row: SourceListItem) {
    await this.router.navigate(['sources', row.id]);
  }
}
