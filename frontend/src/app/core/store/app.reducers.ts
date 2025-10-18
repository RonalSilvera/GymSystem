import { ActionReducerMap } from '@ngrx/store';
import * as Reducers from './reducers';

export class AppState {
  loadingVisible: boolean

  constructor() {
    this.loadingVisible = false
  }

}

export const appReducers: ActionReducerMap<AppState> = {
  loadingVisible: Reducers.loadingVisible,
  
};
