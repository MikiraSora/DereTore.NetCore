﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using DereTore.Applications.ScoreEditor.Controls;
using DereTore.Applications.ScoreEditor.Model;

namespace DereTore.Applications.ScoreEditor.Forms {
    partial class FViewer {

        private void UnregisterEventHandlers() {
            timer.Elapsed -= Timer_Tick;
            btnPlay.Click -= BtnPlay_Click;
            btnStop.Click -= BtnStop_Click;
            btnPause.Click -= BtnPause_Click;
            btnSelectAudio.Click -= BtnSelectAudio_Click;
            btnSelectScore.Click -= BtnSelectScore_Click;
            btnScoreLoad.Click -= BtnScoreLoad_Click;
            btnScoreUnload.Click -= BtnScoreUnload_Click;
            editor.NoteEnteringOrExitingStage -= Editor_NoteUpdated;
            editor.SelectedNoteChanged -= Editor_SelectedNoteChanged;
            editor.MouseDoubleClick -= Editor_MouseDoubleClick;
            editor.MouseWheel -= Editor_MouseWheel;
            Load -= FMain_Load;
            FormClosing -= FViewer_FormClosing;
            trkProgress.ValueChanged -= TrkProgress_ValueChanged;
            trkProgress.MouseDown -= TrkProgress_MouseDown;
            trkProgress.MouseUp -= TrkProgress_MouseUp;
            trkProgress.KeyDown -= TrkProgress_KeyDown;
            trkProgress.KeyUp -= TrkProgress_KeyUp;
            tsbNoteCreate.Click -= TsbNoteCreate_Click;
            tsbNoteEdit.Click -= TsbNoteEdit_Click;
            tsbNoteRemove.Click -= TsbNoteRemove_Click;
            tsbScoreSave.Click -= TsbScoreSave_Click;
            tsbRetimingToNow.Click -= TsbRetimingToNow_Click;
            tsbMakeSync.Click -= TsbMakeSync_Click;
            trkFallingSpeed.ValueChanged -= TrkFallingSpeed_ValueChanged;
        }

        private void RegisterEventHandlers() {
            timer.Elapsed += Timer_Tick;
            btnPlay.Click += BtnPlay_Click;
            btnStop.Click += BtnStop_Click;
            btnPause.Click += BtnPause_Click;
            btnSelectAudio.Click += BtnSelectAudio_Click;
            btnSelectScore.Click += BtnSelectScore_Click;
            btnScoreLoad.Click += BtnScoreLoad_Click;
            btnScoreUnload.Click += BtnScoreUnload_Click;
            editor.NoteEnteringOrExitingStage += Editor_NoteUpdated;
            editor.SelectedNoteChanged += Editor_SelectedNoteChanged;
            editor.MouseDoubleClick += Editor_MouseDoubleClick;
            editor.MouseWheel += Editor_MouseWheel;
            Load += FMain_Load;
            FormClosing += FViewer_FormClosing;
            trkProgress.ValueChanged += TrkProgress_ValueChanged;
            trkProgress.MouseDown += TrkProgress_MouseDown;
            trkProgress.MouseUp += TrkProgress_MouseUp;
            trkProgress.KeyDown += TrkProgress_KeyDown;
            trkProgress.KeyUp += TrkProgress_KeyUp;
            tsbNoteCreate.Click += TsbNoteCreate_Click;
            tsbNoteEdit.Click += TsbNoteEdit_Click;
            tsbNoteRemove.Click += TsbNoteRemove_Click;
            tsbScoreSave.Click += TsbScoreSave_Click;
            tsbRetimingToNow.Click += TsbRetimingToNow_Click;
            tsbMakeSync.Click += TsbMakeSync_Click;
            trkFallingSpeed.ValueChanged += TrkFallingSpeed_ValueChanged;
        }

        private void TrkFallingSpeed_ValueChanged(object sender, EventArgs e) {
            const float a = 2.0f;
            var value = (float)Math.Sqrt((float)trkFallingSpeed.Value / 2);
            value = a / value;
            RenderHelper.FutureTimeWindow = value;
        }

        private void TsbMakeSync_Click(object sender, EventArgs e) {
            var selectedNote = editor.SelectedNote;
            if (selectedNote == null) {
                return;
            }
            Note anotherNote;
            var score = _score;
            if (selectedNote.IsSync) {
                this.ShowMessageBox("The selected note is already synced.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            var basis = FMakeSync.SelectNotes(score, selectedNote, out anotherNote);
            if (anotherNote == null) {
                return;
            }
            if (anotherNote.IsSync) {
                this.ShowMessageBox("The other note is already synced.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            score.MakeSync(selectedNote, anotherNote, basis);
        }

        private void TsbRetimingToNow_Click(object sender, EventArgs e) {
            var selectedNote = editor.SelectedNote;
            var player = _musicPlayer;
            if (selectedNote != null && player != null) {
                var timing = player.CurrentTime.TotalSeconds;
                // Only used for confirmation.
                var temp = selectedNote.Clone();
                temp.HitTiming = timing;
                if (ConfirmNoteEdition(NoteEdition.ResetTiming, selectedNote, temp)) {
                    _score.ResetTimingTo(selectedNote, timing);
                }
            }
        }

        private void TsbScoreSave_Click(object sender, EventArgs e) {
            if (_score == null) {
                return;
            }
            var csvText = _score.SaveToCsv();
            Debug.Print(csvText);
        }

        private void TsbNoteRemove_Click(object sender, EventArgs e) {
            var selectedNote = editor.SelectedNote;
            if (selectedNote == null) {
                return;
            }
            if (ConfirmNoteEdition(NoteEdition.Remove, selectedNote, null)) {
                _score.RemoveNote(selectedNote);
            }
        }

        private void TsbNoteEdit_Click(object sender, EventArgs e) {
            var selectedNote = editor.SelectedNote;
            if (selectedNote == null) {
                return;
            }
            var newNote = FEditNote.ShowEditNote(this, selectedNote);
            if (newNote != null) {
                if (ConfirmNoteEdition(NoteEdition.Edit, selectedNote, newNote)) {
                    _score.EditNote(selectedNote, newNote);
                    propertyGrid.Refresh();
                }
            }
        }

        private void TsbNoteCreate_Click(object sender, EventArgs e) {
            var player = _musicPlayer;
            if (player == null) {
                return;
            }
            var note = FEditNote.ShowCreateNote(this, player.CurrentTime.TotalSeconds);
            if (note != null) {
                note.InitializeAsTap();
                _score.AddNote(note);
            }
        }

        private void Editor_MouseWheel(object sender, MouseEventArgs e) {
            if (!trkProgress.Enabled) {
                return;
            }
            var selectedNote = editor.SelectedNote;
            if (selectedNote == null) {
                int targetValue;
                if (e.Delta > 0) {
                    targetValue = trkProgress.Value + trkProgress.LargeChange;
                    if (targetValue > trkProgress.Maximum) {
                        targetValue = trkProgress.Maximum;
                    }
                    trkProgress.Value = targetValue;
                } else if (e.Delta < 0) {
                    targetValue = trkProgress.Value - trkProgress.LargeChange;
                    if (targetValue < trkProgress.Minimum) {
                        targetValue = trkProgress.Minimum;
                    }
                    trkProgress.Value = targetValue;
                }
            } else {
                var score = _score;
                var newNote = selectedNote.Clone();
                if (e.Delta > 0) {
                    newNote.HitTiming = selectedNote.GetAddTimingResult();
                    if (ConfirmNoteEdition(NoteEdition.Edit, selectedNote, newNote)) {
                        score.AddTiming(selectedNote);
                    }
                } else if (e.Delta < 0) {
                    newNote.HitTiming = selectedNote.GetSubtractTimingResult();
                    if (ConfirmNoteEdition(NoteEdition.Edit, selectedNote, newNote)) {
                        score.SubtractTiming(selectedNote);
                    }
                }
            }
        }

        private void Editor_MouseDoubleClick(object sender, MouseEventArgs e) {
            tsbNoteEdit.PerformClick();
        }

        private void Editor_SelectedNoteChanged(object sender, EventArgs e) {
            propertyGrid.SelectedObject = editor.SelectedNote;
            var anyNoteSelected = editor.SelectedNote != null;
            tsbNoteEdit.Enabled = anyNoteSelected;
            tsbNoteRemove.Enabled = anyNoteSelected;
            tsbMakeSync.Enabled = anyNoteSelected;
            tsbMakeFlick.Enabled = anyNoteSelected;
            tsbMakeHold.Enabled = anyNoteSelected;
            tsbResetToTap.Enabled = anyNoteSelected;
            tsbRetimingToNow.Enabled = anyNoteSelected;
        }

        private void TrkProgress_KeyUp(object sender, KeyEventArgs e) {
            --_userSeekingStack;
            if (_userSeekingStack <= 0) {
                SfxManager.Instance.IsUserSeeking = false;
                if (btnPause.Enabled) {
                    _musicPlayer?.Play();
                }
            }
        }

        private void TrkProgress_KeyDown(object sender, KeyEventArgs e) {
            ++_userSeekingStack;
            SfxManager.Instance.IsUserSeeking = true;
            _musicPlayer?.Pause();
        }

        private void TrkProgress_MouseUp(object sender, MouseEventArgs e) {
            --_userSeekingStack;
            if (_userSeekingStack <= 0) {
                SfxManager.Instance.IsUserSeeking = false;
                if (btnPause.Enabled) {
                    _musicPlayer?.Play();
                }
            }
        }

        private void TrkProgress_MouseDown(object sender, MouseEventArgs e) {
            ++_userSeekingStack;
            SfxManager.Instance.IsUserSeeking = true;
            _musicPlayer?.Pause();
        }

        private void TrkProgress_ValueChanged(object sender, EventArgs e) {
            lock (_liveMusicSyncObject) {
                if (_codeValueChange) {
                    return;
                }
            }
            if (_musicPlayer != null) {
                _musicPlayer.CurrentTime = TimeSpan.FromSeconds(_musicPlayer.TotalLength.TotalSeconds * ((double)(trkProgress.Value - trkProgress.Minimum) / trkProgress.Maximum));
            }
        }

        private void FViewer_FormClosing(object sender, FormClosingEventArgs e) {
            if (btnScoreUnload.Enabled) {
                this.ShowMessageBox("Please unload the score and the audio file before exiting.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Cancel = true;
                return;
            }
            ClosePlayers();
            BtnScoreUnload_Click(this, EventArgs.Empty);
            UnregisterEventHandlers();
        }

        private void Editor_NoteUpdated(object sender, NoteEnteringOrExitingStageEventArgs e) {
            if (e.IsEntering) {
                return;
            }
            var note = e.Note;
            if (chkSfxOn.Checked) {
                if (note.IsFlick) {
                    SfxManager.Instance.PlayWave(_currentFlickHcaFileName);
                } else if (note.IsTap || note.IsHold) {
                    SfxManager.Instance.PlayWave(_currentTapHcaFileName);
                }
            }
        }

        private void FMain_Load(object sender, EventArgs e) {
            InitializeControls();
            PreloadNoteSounds();
            // Enable preview to see more realistic effects.
            editor.IsPreview = true;
            trkFallingSpeed.Value = trkFallingSpeed.Minimum + (int)((float)(trkFallingSpeed.Maximum - trkFallingSpeed.Minimum) / 2);
        }

        private void BtnSelectScore_Click(object sender, EventArgs e) {
            openFileDialog.Filter = ScoreFilter;
            var result = openFileDialog.ShowDialog();
            if (result != DialogResult.Cancel && openFileDialog.FileName.Length > 0) {
                txtScoreFileName.Text = openFileDialog.FileName;
            }
        }

        private void BtnSelectAudio_Click(object sender, EventArgs e) {
            openFileDialog.Filter = AudioFilter;
            var result = openFileDialog.ShowDialog();
            if (result != DialogResult.Cancel && openFileDialog.FileName.Length > 0) {
                txtAudioFileName.Text = openFileDialog.FileName;
            }
        }

        private void BtnScoreUnload_Click(object sender, EventArgs e) {
            timer.Stop();
            BtnStop_Click(sender, e);
            if (_musicPlayer != null) {
                _musicPlayer.PlaybackStopped -= MusicPlayer_PlaybackStopped;
                _musicPlayer.Dispose();
                _musicPlayer = null;
            }
            if (_audioFileStream != null) {
                _audioFileStream.Dispose();
                _audioFileStream = null;
            }
            _score = null;
            editor.Score = null;
            SetControlsEnabled(ViewerState.Initialized);
            lblSong.Text = string.Empty;
            lblTime.Text = string.Empty;
        }

        private void BtnScoreLoad_Click(object sender, EventArgs e) {
            if (!CheckPlayEnvironment()) {
                return;
            }
            var audioFileName = txtAudioFileName.Text;
            var audioFileInfo = new FileInfo(audioFileName);
            var audioFileExtension = audioFileInfo.Extension.ToLowerInvariant();
            if (audioFileExtension == ExtensionAcb) {
                _audioFileStream = File.Open(audioFileName, FileMode.Open, FileAccess.Read);
                _musicPlayer = LiveMusicPlayer.FromAcbStream(_audioFileStream, audioFileName, DefaultCgssDecodeParams);
            } else if (audioFileExtension == ExtensionWav) {
                _audioFileStream = File.Open(audioFileName, FileMode.Open, FileAccess.Read);
                _musicPlayer = LiveMusicPlayer.FromWaveStream(_audioFileStream);
            } else if (audioFileExtension == ExtensionHca) {
                _audioFileStream = File.Open(audioFileName, FileMode.Open, FileAccess.Read);
                _musicPlayer = LiveMusicPlayer.FromHcaStream(_audioFileStream, DefaultCgssDecodeParams);
            } else {
                throw new ArgumentOutOfRangeException(nameof(audioFileExtension), $"Unsupported audio format: '{audioFileExtension}'.");
            }
            _musicPlayer.PlaybackStopped += MusicPlayer_PlaybackStopped;
            var sfxDirName = string.Format(SoundEffectAudioDirectoryNameFormat, cboSoundEffect.SelectedIndex.ToString("00"));
            _currentTapHcaFileName = $"{sfxDirName}/{TapHcaName}";
            _currentFlickHcaFileName = $"{sfxDirName}/{FlickHcaName}";
            Score score;
            var scoreFileName = txtScoreFileName.Text;
            var scoreFileExtension = new FileInfo(scoreFileName).Extension.ToLowerInvariant();
            if (scoreFileExtension == ExtensionBdb) {
                score = Score.FromBdbFile(scoreFileName, (Difficulty)(cboDifficulty.SelectedIndex + 1));
            } else if (scoreFileExtension == ExtensionCsv) {
                score = Score.FromCsvFile(scoreFileName);
            } else {
                throw new ArgumentException("What?", nameof(scoreFileExtension));
            }
            _score = score;
            editor.Score = _score;
            SetControlsEnabled(ViewerState.Loaded);
            lblSong.Text = string.Format(SongTipFormat, _musicPlayer.HcaName);
            timer.Start();
        }

        private void MusicPlayer_PlaybackStopped(object sender, NAudio.Wave.StoppedEventArgs e) {
            BtnStop_Click(this, EventArgs.Empty);
        }

        private void BtnStop_Click(object sender, EventArgs e) {
            if (_musicPlayer == null) {
                return;
            }
            // Neccessary. Sometimes the stack just doesn't clear.
            _userSeekingStack = 0;
            _musicPlayer.Stop();
            _musicPlayer.CurrentTime = TimeSpan.Zero;
            SetControlsEnabled(ViewerState.Loaded);
            lock (_liveMusicSyncObject) {
                _codeValueChange = true;
                trkProgress.Value = trkProgress.Minimum;
                _codeValueChange = false;
            }
            if (!(editor.Disposing || editor.IsDisposed)) {
                editor.SetTime(TimeSpan.Zero);
                editor.Invalidate();
            }
            editor.MouseEventsEnabled = true;
        }

        private void BtnPause_Click(object sender, EventArgs e) {
            _musicPlayer.Pause();
            SetControlsEnabled(ViewerState.LoadedAndPaused);
            editor.MouseEventsEnabled = true;
        }

        private void BtnPlay_Click(object sender, EventArgs e) {
            if (_musicPlayer == null) {
                return;
            }
            if (_musicPlayer.IsPaused) {
                SetControlsEnabled(ViewerState.LoadedAndPlaying);
            } else {
                SetControlsEnabled(ViewerState.LoadedAndPlaying);
            }
            editor.MouseEventsEnabled = false;
            _musicPlayer.Play();
            lblTime.Text = $"{_musicPlayer.CurrentTime}/{_musicPlayer.TotalLength}";
        }

        private void Timer_Tick(object sender, EventArgs e) {
            var player = _musicPlayer;
            if (player == null) {
                return;
            }
            var elapsed = player.CurrentTime;
            var total = player.TotalLength;
            if (elapsed >= total) {
                BtnStop_Click(this, EventArgs.Empty);
                return;
            }
            var ratio = elapsed.TotalSeconds / total.TotalSeconds;
            var val = (int)(ratio * (trkProgress.Maximum - trkProgress.Minimum)) + trkProgress.Minimum;
            lock (_liveMusicSyncObject) {
                _codeValueChange = true;
                trkProgress.Value = val;
                _codeValueChange = false;
            }
            lblTime.Text = $"{elapsed}/{player.TotalLength}";
            editor.SetTime(elapsed);
            editor.Invalidate();
            editor.JudgeNotesEnteringOrExiting(elapsed);
        }

    }
}
