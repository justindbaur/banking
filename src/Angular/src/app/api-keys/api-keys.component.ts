import { Component } from '@angular/core';
import { ApiService } from '../services/api.service';
import { Observable } from 'rxjs';
import { ApiKey } from '../services/api.service';

@Component({
  selector: 'app-api-keys',
  templateUrl: './api-keys.component.html',
  styleUrls: ['./api-keys.component.scss']
})
export class ApiKeysComponent {
  displayedColumns = ["name", "created", "lastUsed", "expiration"];
  apiKeys$: Observable<ApiKey[]>;

  constructor(private apiService: ApiService) {
    this.apiKeys$ = this.apiService.getApiKeys();
  }
}
