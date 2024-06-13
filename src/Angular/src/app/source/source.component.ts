import { Component } from '@angular/core';
import { SourceListItem, SourcesService } from './sources.service';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Observable, lastValueFrom, map, shareReplay, switchMap } from 'rxjs';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatGridListModule } from '@angular/material/grid-list';

@Component({
  selector: 'app-source',
  standalone: true,
  template: `<ng-container *ngIf="source$ | async as source">
    <!-- Grid this out -->
    <mat-grid-list cols="2" rowHeight="2:1">
      <mat-grid-tile>
        <mat-card>
          <mat-card-content>
            <p>{{ source.displayName }}</p>
          </mat-card-content>
        </mat-card>
      </mat-grid-tile>
      <mat-grid-tile>
        <mat-card>
          <mat-card-content>
            <p>{{ source.enabled }}</p>
          </mat-card-content>
        </mat-card>
      </mat-grid-tile>
    </mat-grid-list>
  </ng-container>`,
  imports: [RouterModule, CommonModule, MatCardModule, MatGridListModule],
  providers: [SourcesService],
})
export class SourceComponent {
  source$: Observable<SourceListItem>;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly sourcesService: SourcesService
  ) {
    const sourceId$ = this.route.paramMap.pipe(
      map((p) => p.get('id')!),
      shareReplay(1)
    );

    this.source$ = sourceId$.pipe(
      switchMap((id) => this.sourcesService.get$(id))
    );
  }

  async delete(sourceId: string) {
    await lastValueFrom(this.sourcesService.delete$(sourceId));
  }
}
