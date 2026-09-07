import numpy as np
import matplotlib.pyplot as plt
import io
import logging

class Telemetria:

    @staticmethod
    def analizza(car_dict):

        try:
            x = np.array(car_dict['x'])
            z = np.array(car_dict['z'])
            t = np.array(car_dict['tempo'])
            v = np.array(car_dict['vel'])
            rpm = np.array(car_dict['rpm'])
            temp = np.array(car_dict['temp'])
            marce = np.array(car_dict['marcia'], dtype=int)
            
            v_ = v * 3.6
            v_media = np.mean(v_)
            v_max = np.max(v_)

            tempo_0_100 = None
            tmp = np.where(v_ >= 100.0)[0]
            if tmp.size > 0:
                tempo_0_100 = t[tmp[0]]

            dv = np.diff(v)
            dt = np.diff(t)
            a = np.divide(dv, dt, out=np.zeros_like(dv))
            a_max = np.max(a)

            temp_media = np.mean(temp)

            rpm_max = np.max(rpm)
            marcia_top = np.bincount(marce).argmax()

            i_freno = np.where(a < -0.5)[0]
            if i_freno.size > 0:
                iniziofren = i_freno[0]
                finefren = iniziofren
                while (finefren + 1 < len(v) and v[finefren + 1] < v[finefren]):
                    finefren += 1
                dist_fren = (x[finefren] - x[iniziofren])
            else:
                dist_fren = None

            #traiettria
            fig1, ax1 = plt.subplots(figsize=(10, 7))
            ax1.plot(x, z, label='Traiettoria', color='blue', linewidth=2)
            ax1.scatter(x[0], z[0], color='green', s=100, label='Partenza', zorder=5)
            ax1.scatter(x[-1], z[-1], color='red', s=100, label='Arrivo', zorder=5)
            ax1.set_xlabel('X (m)')
            ax1.set_ylabel('Z (m)')
            ax1.set_title('Traiettoria del veicolo')
            ax1.legend()
            ax1.set_ylim(bottom=-50, top=(z[-1] + 50))
            ax1.grid(True, linestyle='--', alpha=0.6)
            buf1 = io.BytesIO()
            fig1.savefig(buf1, format='png')
            plt.close(fig1)

            #velocita
            fig2, ax2 = plt.subplots(figsize=(10, 7))
            ax2.plot(t, v_, label='Velocità', color='purple', linewidth=1.5)
            ax2.set_ylabel('Velocità (km/h)')
            ax2.set_xlabel('Tempo (s)')
            ax2.set_title('Velocità nel tempo')
            ax2.grid(True, linestyle=':', alpha=0.7)
            buf2 = io.BytesIO()
            fig2.savefig(buf2, format='png')
            plt.close(fig2)

            #giri del motore
            fig3, ax3 = plt.subplots(figsize=(10, 7))
            ax3.plot(t, rpm, label='Giri Motore', color='red', linewidth=1.5)
            ax3.set_xlabel('Tempo')
            ax3.set_ylabel('Giri al Minuto')
            ax3.set_title('RPM')
            ax3.set_ylim(bottom=0, top=max(rpm_max * 1.1, 7000))
            ax3.grid(True, linestyle='--', alpha=0.5)
            buf3 = io.BytesIO()
            fig3.savefig(buf3, format='png')
            plt.close(fig3)



            return {
                "v_media": v_media,
                "v_max": v_max,
                "a_max": a_max,
                "t_accela": tempo_0_100,
                "t_media": temp_media,
                "rpm_max": int(rpm_max),
                "marcia": int(marcia_top),
                "dist": x[-1] if x.size > 0 else 0,
                "img_data": buf1.getvalue(),
                "img_data2": buf2.getvalue(),
                "img_data3": buf3.getvalue(),
                "distanza_frenata": dist_fren
            }

        except Exception as e:
            logging.error(f"Errore analizzatore: {e}")
            return None